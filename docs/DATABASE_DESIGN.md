# تصميم قاعدة البيانات — مِيزاد (DATABASE_DESIGN.md)

> التصميم المرجعي لقاعدة بيانات SQL Server. يُنفذ في Phase 2 عبر EF Core Code-First + Migrations.
> أي انحراف أثناء التنفيذ يُوثق في §12 «سجل التغييرات» مع سببه.
> القرارات: D-004, D-005, D-006, D-007, D-008, D-010, D-012, D-041. القواعد: [BUSINESS_RULES.md](BUSINESS_RULES.md).

---

## 1. الاصطلاحات العامة

| البند | القاعدة |
|---|---|
| المحرك | SQL Server 2019+ (التطوير: 2025 Developer). يعمل على Express (حد 50GB في 2025). |
| اسم القاعدة | `Mizad` (قابل للضبط). اختبارات التكامل: `Mizad_IT_<guid>`. |
| الترتيب | `Arabic_100_CI_AI` على مستوى القاعدة (D-041) — للفرز العربي وعدم حساسية حالة الأحرف اللاتينية فقط. **لا يتجاهل التشكيل ولا يوحّد أ/ا أو ة/ه** (مُتحقق منه): البحث العربي عبر `SearchName` المُطبَّع حصراً. |
| المخطط | `dbo` لكل الجداول (بساطة الاستعلامات الخام). |
| المفاتيح | `Id int IDENTITY` للبيانات الرئيسية والمستندات؛ `bigint` للجداول عالية الحجم (الحركات، القيود، التدقيق، التسويات). |
| المبالغ | `decimal(19,4)` — تُسمى *Money* أدناه. |
| تكلفة الوحدة | `decimal(19,6)` — *UnitCost*. |
| الكميات | `decimal(18,3)` — *Qty*. |
| معاملات التحويل | `decimal(18,6)` — *Factor*. |
| النسب | `decimal(9,4)` — *Pct*. |
| النصوص | `nvarchar(n)`: الأسماء 200، الأكواد 50، الهواتف 30، الملاحظات 1000، أرقام المستندات 30. |
| التواريخ التجارية | `datetime2(0)` بالتوقيت المحلي (`DocumentDate`)؛ الاستحقاق `date`. |
| طوابع التدقيق | `CreatedAtUtc`, `UpdatedAtUtc` `datetime2(3)`؛ `CreatedByUserId`, `UpdatedByUserId` `int NULL` FK → Users. |
| التزامن | `RowVersion rowversion` على كل جذر قابل للتعديل (الأصناف، الأطراف، رؤوس المستندات، الأرصدة، الإعدادات، المستخدمين). |
| الحذف | `ON DELETE NO ACTION` (Restrict) لكل العلاقات، باستثناء أسطر المسودات ← رأسها (Cascade؛ آمن لأن المعتمد لا يُحذف). |
| الحذف المنطقي | البيانات الرئيسية `IsActive bit`. المستندات المعتمدة لا تُحذف أبداً (BR-003). |
| التعدادات | تخزن `tinyint` مع `CHECK` على القيم المسموحة؛ القيم موثقة في §10. |
| أنواع المستندات المصدر | `SourceDocType varchar(30)` بقيم ثابتة (§10.1) + `SourceDocId int`. |

**أعمدة المستند القياسية** (تتكرر في كل رأس مستند وتُختصر أدناه بـ «＋أعمدة المستند»):
`Number nvarchar(30) NULL` (فريد مُفلتر `WHERE Number IS NOT NULL`)، `Status tinyint` (Draft=0, Posted=1, Voided=2)، `DocumentDate datetime2(0)`، `Notes nvarchar(1000) NULL`، `ClientRequestId uniqueidentifier` (فريد)، `PostedAtUtc`, `PostedByUserId`, `VoidedAtUtc`, `VoidedByUserId`, `VoidReason nvarchar(500) NULL`، أعمدة التدقيق، `RowVersion`.
قيد: `CHECK (Status = 0 OR Number IS NOT NULL)`.

---

## 2. خريطة الجداول حسب الوحدة

| الوحدة | الجداول |
|---|---|
| الأمان | Users, Roles, RolePermissions, AuditLogs |
| الإدارة | Settings, DocumentSequences, BackupHistory |
| الأصناف | Categories, Units, Products, ProductUnits, ProductBarcodes, ProductImages |
| المخزون | Warehouses, StockBalances, StockMovements, AdjustmentReasons, StockAdjustments, StockAdjustmentLines, InventoryCounts, InventoryCountLines, StockTransfers, StockTransferLines |
| الأطراف | Customers, Suppliers, OpeningBalances, PartyLedgerEntries, PartySettlements |
| المبيعات | SalesInvoices, SalesInvoiceLines |
| المشتريات | PurchaseInvoices, PurchaseInvoiceLines |
| المرتجعات | ReturnReasons, SalesReturns, SalesReturnLines, PurchaseReturns, PurchaseReturnLines |
| الخزينة | CashAccounts, CashTransactions, CashVouchers, VoucherReasons, ExpenseCategories, Expenses, CashTransfers, CashierShifts |

```
Categories ─┐            ┌─ ProductBarcodes
Units ──────┼─ Products ─┼─ ProductUnits ─┘
            │            └─ ProductImages
Warehouses ─┴─ StockBalances / StockMovements ◄── (كل مستند مخزني)

Customers ─┬─ SalesInvoices ─ SalesInvoiceLines ◄─ SalesReturnLines ─ SalesReturns
           └─ PartyLedgerEntries ◄─ PartySettlements (Debit↔Credit)
Suppliers ─┬─ PurchaseInvoices ─ PurchaseInvoiceLines ◄─ PurchaseReturnLines ─ PurchaseReturns
           └─ PartyLedgerEntries
CashAccounts ─ CashTransactions ◄── (CashVouchers / Expenses / CashTransfers / CashierShifts)
```

---

## 3. الأمان والإدارة

### Users
| العمود | النوع | قيود |
|---|---|---|
| Id | int PK | |
| UserName | nvarchar(50) | فريد (الترتيب CI) |
| FullName | nvarchar(150) | |
| PasswordHash | varchar(200) | صيغة `v1$iter$salt$hash` (D-017) |
| RoleId | int FK → Roles | |
| IsActive | bit | |
| MustChangePassword | bit | |
| FailedLoginCount | int | ≥ 0 |
| LockoutEndUtc | datetime2(3) NULL | |
| LastLoginAtUtc | datetime2(3) NULL | |
| + التدقيق، RowVersion | | |

### Roles
`Id`, `Name nvarchar(100)` فريد، `Description nvarchar(300)`، `IsSystem bit` (مدير النظام = 1، لا يُعدل)، التدقيق، RowVersion.

### RolePermissions
`RoleId int FK`, `PermissionCode varchar(100)` — PK مركب (RoleId, PermissionCode). الرموز من الكتالوج الثابت في الكود (BUSINESS_RULES §9).

### AuditLogs
`Id bigint PK`, `OccurredAtUtc datetime2(3)`, `UserId int NULL FK`, `UserName nvarchar(50)`, `MachineName nvarchar(100)`, `Action varchar(100)`, `EntityType varchar(100) NULL`, `EntityId varchar(50) NULL`, `Summary nvarchar(500)`, `DetailsJson nvarchar(max) NULL`.
فهارس: (OccurredAtUtc DESC)، (EntityType, EntityId)، (UserId, OccurredAtUtc).

### Settings
`Key varchar(100) PK`, `Value nvarchar(max) NULL`, `UpdatedAtUtc`, `UpdatedByUserId`, `RowVersion`. المفاتيح في BUSINESS_RULES §13.

### DocumentSequences
`DocumentType varchar(30) PK`, `Prefix nvarchar(10)`, `NextNumber int` (≥ 1)، `Padding tinyint` (افتراضي 6)، `RowVersion`.
التخصيص: `UPDATE DocumentSequences WITH (UPDLOCK, ROWLOCK) SET NextNumber = NextNumber + 1 OUTPUT deleted.NextNumber, deleted.Prefix, deleted.Padding WHERE DocumentType = @t` داخل معاملة الاعتماد.

### BackupHistory
`Id int PK`, `StartedAtUtc`, `CompletedAtUtc NULL`, `BackupType tinyint` (Manual, Scheduled, PreRestore, PreMigration)، `FilePath nvarchar(500)`, `SizeBytes bigint NULL`, `Status tinyint` (Running, Succeeded, Failed, VerifyFailed)، `Verified bit`, `ErrorMessage nvarchar(1000) NULL`, `MachineName nvarchar(100)`, `UserId int NULL`.

---

## 4. الأصناف

### Categories
`Id`, `Name nvarchar(150)` فريد، `SortOrder int`, `IsActive`, التدقيق، RowVersion.

### Units
`Id`, `Name nvarchar(50)` فريد، `IsActive`. بذرة: حبة، قطعة، كجم، غرام، لتر، علبة، كرتون، عبوة، كيس، متر.

### Products
| العمود | النوع | قيود/ملاحظات |
|---|---|---|
| Id | int PK | |
| Code | nvarchar(50) | فريد (SKU؛ يولد تلقائياً إن تُرك فارغاً) |
| Name | nvarchar(200) | |
| SearchName | nvarchar(200) | مُطبَّع (D-041)؛ مفهرس |
| CategoryId | int FK | |
| BaseUnitId | int FK → Units | |
| MinStockQty | Qty | ≥ 0 (بالوحدة الأساسية) |
| AverageCost | UnitCost | ≥ 0؛ **يكتبه محرك المخزون فقط** |
| LastPurchaseCost | UnitCost NULL | |
| HasMultipleUnits | bit | |
| IsActive | bit | |
| Notes | nvarchar(1000) NULL | |
| + التدقيق، RowVersion | | |
فهارس: (SearchName)، (CategoryId, IsActive).

### ProductUnits
`Id`, `ProductId FK`, `UnitId FK`, `Factor Factor` (`CHECK > 0`)، `IsBaseUnit bit`، `RetailPrice Money ≥ 0`, `WholesalePrice Money NULL ≥ 0`, `IsDefaultSale bit`, `IsDefaultPurchase bit`, `IsActive`.
قيود: فريد (ProductId, UnitId)؛ فريد مُفلتر (ProductId) `WHERE IsBaseUnit = 1`؛ `CHECK (IsBaseUnit = 0 OR Factor = 1)`.

### ProductBarcodes
`Id`, `ProductUnitId FK`, `Barcode nvarchar(50)` **فريد على مستوى النظام**.

### ProductImages
`ProductId int PK/FK`, `ImageData varbinary(max)`, `ThumbnailData varbinary(max)`, `ContentType varchar(50)`, `UpdatedAtUtc`. (D-031)

---

## 5. المخزون

### Warehouses
`Id`, `Code nvarchar(20)` فريد، `Name nvarchar(150)` فريد، `IsDefault bit` (فريد مُفلتر `WHERE IsDefault=1`)، `IsActive`، التدقيق.

### StockBalances
| العمود | النوع |
|---|---|
| ProductId | int FK — PK مركب |
| WarehouseId | int FK — PK مركب |
| Quantity | Qty |
| RowVersion | rowversion |
يكتبه `IInventoryEngine` فقط (D-006). الصادر بتحديث ذري مشروط (BR-012).

### StockMovements (إلحاقي — لا UPDATE ولا DELETE)
| العمود | النوع | ملاحظات |
|---|---|---|
| Id | bigint PK | |
| ProductId | int FK | |
| WarehouseId | int FK | |
| MovementType | tinyint | §10.2 |
| OccurredAt | datetime2(3) | تاريخ المستند التجاري |
| Quantity | Qty | موجب وارد / سالب صادر؛ `CHECK <> 0` |
| UnitCost | UnitCost | |
| TotalCost | Money | = Quantity × UnitCost (بإشارتها) |
| BalanceAfter | Qty | رصيد المخزن بعد الحركة |
| TotalOnHandAfter | Qty | إجمالي كل المخازن بعد الحركة |
| AverageCostAfter | UnitCost | |
| SourceDocType | varchar(30) | |
| SourceDocId | int | |
| SourceLineId | int NULL | |
| DocumentNumber | nvarchar(30) | |
| Notes | nvarchar(500) NULL | |
| CreatedAtUtc, CreatedByUserId | | |
فهارس: (ProductId, OccurredAt, Id) INCLUDE (Quantity, BalanceAfter)؛ (WarehouseId, ProductId)؛ (SourceDocType, SourceDocId)؛ (MovementType, OccurredAt).

### AdjustmentReasons
`Id`, `Name nvarchar(100)` فريد، `Direction tinyint` (In, Out, Both)، `IsSystem bit`, `IsActive`. بذرة: رصيد افتتاحي (نظامي)، زيادة جرد، نقص جرد، تالف، هدايا وعينات، استهلاك داخلي، تصحيح إدخال.

### StockAdjustments ＋أعمدة المستند
`WarehouseId FK`, `ReasonId FK`, `InventoryCountId int NULL FK`.
### StockAdjustmentLines
`Id`, `StockAdjustmentId FK (Cascade)`, `LineNo`, `ProductId FK`, `SystemQty Qty` (لقطة عند الاعتماد)، `QuantityDelta Qty` (`<> 0`)، `UnitCost UnitCost` (المتوسط، أو المُدخل لسبب «رصيد افتتاحي»)، `LineValue Money`.

### InventoryCounts ＋أعمدة المستند (Status: Open=0, Submitted=3, Approved=1, Cancelled=2)
`WarehouseId FK`, `CategoryId int NULL FK` (النطاق)، `SnapshotAtUtc`, `ApprovedAtUtc NULL`, `ApprovedByUserId NULL`, `TreatUncountedAsZero bit`, `StockAdjustmentId int NULL FK`.
### InventoryCountLines
`Id`, `InventoryCountId FK`, `ProductId FK`, `SnapshotQty Qty`, `CountedQty Qty NULL`, `UnitCost UnitCost NULL` (عند الاعتماد)، `Notes`. فريد (InventoryCountId, ProductId).

### StockTransfers ＋أعمدة المستند
`FromWarehouseId FK`, `ToWarehouseId FK`, `CHECK (FromWarehouseId <> ToWarehouseId)`.
### StockTransferLines
`Id`, `StockTransferId FK`, `LineNo`, `ProductId`, `ProductUnitId`, `Factor`, `Quantity Qty > 0`, `BaseQuantity Qty > 0`.

---

## 6. الأطراف والذمم (D-007)

### Customers
| العمود | النوع | ملاحظات |
|---|---|---|
| Id | int PK | |
| Code | nvarchar(20) | فريد |
| Name / SearchName | nvarchar(200) | SearchName مفهرس |
| Phone / Phone2 | nvarchar(30) NULL | Phone مفهرس |
| Address | nvarchar(300) NULL | |
| TaxNumber | nvarchar(50) NULL | |
| PriceTier | tinyint | Retail=0, Wholesale=1 |
| AllowCredit | bit | |
| CreditLimit | Money NULL | NULL = بلا حد؛ ≥ 0 |
| PaymentTermDays | int NULL | ≥ 0 |
| IsWalkIn | bit | فريد مُفلتر `WHERE IsWalkIn = 1`؛ `CHECK (IsWalkIn = 0 OR AllowCredit = 0)` |
| IsActive | bit | |
| Notes | nvarchar(1000) NULL | |
| + التدقيق، RowVersion | | |

### Suppliers
`Id`, `Code` فريد، `Name`, `SearchName`, `ContactPerson nvarchar(150)`, `Phone`, `Phone2`, `Email nvarchar(150)`, `Address`, `TaxNumber`, `Description nvarchar(300)` (مثلاً «مورد عام للمواد الغذائية» كما في UI-09)، `PaymentTermDays`, `IsActive`, `Notes`، التدقيق، RowVersion.

### OpeningBalances ＋أعمدة المستند
`PartyType tinyint` (Customer=1, Supplier=2)، `CustomerId int NULL FK`, `SupplierId int NULL FK`, `Side tinyint` (Debit=1, Credit=2)، `Amount Money > 0`, `DueDate date NULL`.
`CHECK ((PartyType=1 AND CustomerId IS NOT NULL AND SupplierId IS NULL) OR (PartyType=2 AND SupplierId IS NOT NULL AND CustomerId IS NULL))`.

### PartyLedgerEntries (إلحاقي؛ يُحدَّث فقط `SettledAmount` و`IsReversed`)
| العمود | النوع | ملاحظات |
|---|---|---|
| Id | bigint PK | |
| PartyType | tinyint | 1 عميل، 2 مورد |
| CustomerId / SupplierId | int NULL FK | نفس CHECK أعلاه |
| EntryDate | datetime2(0) | تاريخ المستند |
| DueDate | date NULL | للبنود المدينة (عميل) / الدائنة (مورد) |
| EntryType | tinyint | §10.3 |
| Debit | Money | ≥ 0 |
| Credit | Money | ≥ 0؛ `CHECK ((Debit > 0 AND Credit = 0) OR (Credit > 0 AND Debit = 0))` |
| SettledAmount | Money | 0 ≤ ≤ max(Debit, Credit)؛ ذاكرة مؤقتة = Σ التسويات الفعالة |
| SourceDocType / SourceDocId | | فريد (SourceDocType, SourceDocId, EntryType) |
| DocumentNumber | nvarchar(30) | |
| Description | nvarchar(300) | |
| ReversalOfEntryId | bigint NULL FK self | |
| IsReversed | bit | |
| CreatedAtUtc, CreatedByUserId | | |
فهارس: (CustomerId, EntryDate, Id) INCLUDE (Debit, Credit, SettledAmount)؛ (SupplierId, EntryDate, Id) INCLUDE (...)؛ مُفلتر للبنود المفتوحة: (CustomerId, DueDate) `WHERE SettledAmount < Debit + Credit AND IsReversed = 0`.

### PartySettlements
`Id bigint PK`, `DebitEntryId bigint FK`, `CreditEntryId bigint FK`, `Amount Money > 0`, `IsAuto bit` (FIFO أم صريح)، `CreatedAtUtc`, `ReversedAtUtc NULL`.
فهارس: (DebitEntryId) و(CreditEntryId) `WHERE ReversedAtUtc IS NULL`.
**ثابت المطابقة:** `SettledAmount(entry) = Σ Amount` للتسويات الفعالة المرتبطة به، وطرفا التسوية لنفس الطرف (يُفرض في الخدمة ويُختبر).

---

## 7. المبيعات والمشتريات

### SalesInvoices ＋أعمدة المستند
| العمود | النوع | ملاحظات |
|---|---|---|
| CustomerId | int FK | |
| WarehouseId | int FK | |
| PriceTier | tinyint | لقطة |
| DueDate | date NULL | |
| SubTotal | Money | |
| LinesDiscountTotal | Money | |
| InvoiceDiscountType | tinyint | None=0, Amount=1, Percent=2 |
| InvoiceDiscountValue | Money | القيمة المدخلة (مبلغ أو نسبة) |
| InvoiceDiscountAmount | Money | |
| TaxRate | Pct | لقطة |
| TaxAmount | Money | |
| Total | Money | ≥ 0 |
| PaidAtSale | Money | 0 ≤ ≤ Total |
| PaymentMethod | tinyint NULL | للجزء المدفوع |
| CashAccountId | int NULL FK | |
| CashTendered / ChangeGiven | Money NULL | للعرض والطباعة فقط |
| CostTotal | Money | Σ CostTotal للأسطر (عند الاعتماد) |
| ShiftId | int NULL FK | |
فهارس: (DocumentDate, Status)، (CustomerId, DocumentDate)، (Status) مُفلتر للمسودات.

### SalesInvoiceLines
| العمود | النوع | ملاحظات |
|---|---|---|
| Id | int PK | |
| SalesInvoiceId | int FK (Cascade للمسودة) | |
| LineNo | int | |
| ProductId / ProductUnitId | int FK | |
| ProductName / UnitName | nvarchar | لقطة |
| Factor | Factor | لقطة |
| Quantity / BaseQuantity | Qty | > 0 |
| UnitPrice | Money | ≥ 0 |
| LineSubTotal / LineDiscount / LineNet | Money | |
| AllocatedInvoiceDiscount / TaxAmount / LineTotal | Money | BR-091 |
| UnitCost | UnitCost | لقطة عند الاعتماد (BR-011) |
| CostTotal | Money | |
| ReturnedBaseQuantity | Qty | 0 ≤ ≤ BaseQuantity؛ تحديث ذري (BR-041) |
| ReturnedAmount | Money | Σ قيم المرتجع (لقاعدة الإرجاع الكامل BR-042) |
`CHECK (ReturnedBaseQuantity <= BaseQuantity)`.

### PurchaseInvoices ＋أعمدة المستند
`SupplierId FK`, `SupplierInvoiceNumber nvarchar(50) NULL`, `WarehouseId FK`, `DueDate`, `SubTotal`, `LinesDiscountTotal`, `InvoiceDiscountType/Value/Amount`, `Total`, `PaidAtPurchase`, `PaymentMethod NULL`, `CashAccountId NULL`.
فريد مُفلتر: (SupplierId, SupplierInvoiceNumber) `WHERE SupplierInvoiceNumber IS NOT NULL AND Status <> 2` (BR-032).

### PurchaseInvoiceLines
`Id`, `PurchaseInvoiceId`, `LineNo`, `ProductId`, `ProductUnitId`, `ProductName`, `UnitName`, `Factor`, `Quantity`, `BaseQuantity`, `UnitCost Money` (للوحدة المدخلة)، `DiscountPercent Pct`, `LineDiscount`, `LineNet`, `AllocatedInvoiceDiscount`, `LineTotal`, `LandedUnitCost UnitCost` (لكل وحدة أساسية — BR-031)، `ReturnedBaseQuantity`, `ReturnedAmount`.

---

## 8. المرتجعات

### ReturnReasons
`Id`, `Name nvarchar(100)` فريد، `AppliesTo tinyint` (Sales, Purchase, Both)، `DefaultRestock bit`, `IsActive`.

### SalesReturns ＋أعمدة المستند
`SalesInvoiceId FK`, `CustomerId FK`, `WarehouseId FK`, `ReturnReasonId FK`, `Restock bit`, `SettlementMethod tinyint` (CashRefund=1, CustomerCredit=2)، `Total Money`, `CostTotal Money`, `AppliedToInvoice Money`, `RefundAmount Money`, `RefundCashAccountId NULL FK`, `RefundVoucherId NULL FK → CashVouchers`, `TargetSalesInvoiceId NULL FK` (تسوية مع فاتورة أخرى محددة).
### SalesReturnLines
`Id`, `SalesReturnId FK`, `SalesInvoiceLineId FK`, `ProductId`, `ProductUnitId`, `Factor`, `Quantity`, `BaseQuantity > 0`, `UnitNetPrice Money` (لكل وحدة أساسية)، `LineTotal Money`, `UnitCost UnitCost` (من سطر البيع)، `CostTotal Money`.

### PurchaseReturns ＋أعمدة المستند
`PurchaseInvoiceId FK`, `SupplierId FK`, `WarehouseId FK`, `ReturnReasonId FK`, `SettlementMethod tinyint` (CashRefund=1, SupplierCredit=2)، `Total`, `AppliedToInvoice`, `RefundAmount`, `RefundCashAccountId NULL`, `RefundVoucherId NULL`.
### PurchaseReturnLines
`Id`, `PurchaseReturnId`, `PurchaseInvoiceLineId FK`, `ProductId`, `ProductUnitId`, `Factor`, `Quantity`, `BaseQuantity`, `UnitCost UnitCost` (= LandedUnitCost)، `LineTotal Money`.

---

## 9. الخزينة

### CashAccounts
`Id`, `Name nvarchar(100)` فريد، `AccountType tinyint` (Cash=1, Bank=2)، `IsDefault bit` (فريد مُفلتر لكل نوع)، `IsActive`، التدقيق، RowVersion (يُقفل `UPDLOCK` عند فحص السالب BR-061).

### CashTransactions (إلحاقي)
| العمود | النوع |
|---|---|
| Id | bigint PK |
| CashAccountId | int FK |
| TxDate | datetime2(0) |
| AmountIn / AmountOut | Money ≥ 0؛ أحدهما فقط > 0 |
| TxType | tinyint (§10.4) |
| PaymentMethod | tinyint |
| PartyType / CustomerId / SupplierId | NULL |
| SourceDocType / SourceDocId / DocumentNumber | |
| Description | nvarchar(300) |
| ShiftId | int NULL FK |
| ReversalOfId | bigint NULL FK self |
| CreatedAtUtc, CreatedByUserId | |
فهارس: (CashAccountId, TxDate, Id) INCLUDE (AmountIn, AmountOut)؛ (SourceDocType, SourceDocId)؛ (ShiftId).

### CashVouchers ＋أعمدة المستند
`VoucherType tinyint` (Receipt=1, Payment=2)، `PartyType tinyint` (None=0, Customer=1, Supplier=2)، `CustomerId NULL`, `SupplierId NULL`, `VoucherReasonId NULL FK` (إلزامي عند None)، `Amount Money > 0`, `PaymentMethod tinyint`, `CashAccountId FK`, `Description nvarchar(300)`, `IsAutoGenerated bit`, `OriginDocType varchar(30) NULL`, `OriginDocId int NULL` (الفاتورة/المرتجع الذي ولّده)، `ShiftId NULL`.
البادئة: RC- للقبض، PV- للصرف.

### VoucherReasons
`Id`, `Name`, `VoucherType tinyint` (Receipt, Payment, Both)، `AffectsProfit tinyint` (None=0, OtherIncome=1)، `IsActive`. بذرة: إيداع مالك، سحب مالك، إيرادات أخرى، أخرى.

### ExpenseCategories
`Id`, `Name` فريد، `IconKey varchar(50) NULL`, `IsActive`. بذرة (من UI-10): إيجار ومرافق، رواتب وأجور، نقل وشحن، مصاريف إدارية، صيانة، ضيافة، كهرباء وماء، أخرى.

### Expenses ＋أعمدة المستند
`ExpenseCategoryId FK`, `Amount Money > 0`, `PaymentMethod`, `CashAccountId FK`, `Payee nvarchar(200) NULL`, `Description nvarchar(300)`, `ShiftId NULL`.

### CashTransfers ＋أعمدة المستند
`FromAccountId FK`, `ToAccountId FK` (`<>`)، `Amount Money > 0`.

### CashierShifts
`Id`, `Number nvarchar(30)`, `UserId FK`, `CashAccountId FK`, `Status tinyint` (Open, Closed)، `OpenedAt`, `OpeningFloat Money`, `ClosedAt NULL`, `ExpectedCash Money NULL`, `CountedCash Money NULL`, `Difference Money NULL`, `ClosedByUserId NULL`, `Notes`, RowVersion.
فريد مُفلتر (UserId) `WHERE Status = 0`.

---

## 10. قيم التعدادات

### 10.1 SourceDocType
`SalesInvoice`, `PurchaseInvoice`, `SalesReturn`, `PurchaseReturn`, `CashVoucher`, `Expense`, `StockAdjustment`, `InventoryCount`, `StockTransfer`, `CashTransfer`, `OpeningBalance`, `CashierShift`.

### 10.2 MovementType (StockMovements)
1 OpeningBalance · 2 Purchase · 3 PurchaseReturn · 4 Sale · 5 SaleReturn · 6 AdjustmentIn · 7 AdjustmentOut · 8 TransferIn · 9 TransferOut · 10 VoidReversal.

### 10.3 EntryType (PartyLedgerEntries)
1 OpeningBalance · 2 SalesInvoice · 3 SalesReturn · 4 Receipt (تحصيل من عميل / استرداد من مورد) · 5 Payment (تسديد لمورد / رد لعميل) · 6 PurchaseInvoice · 7 PurchaseReturn · 8 Reversal.

### 10.4 TxType (CashTransactions)
1 SaleReceipt · 2 CustomerReceipt · 3 CustomerRefund · 4 SupplierPayment · 5 SupplierRefund · 6 Expense · 7 OtherReceipt · 8 OtherPayment · 9 TransferIn · 10 TransferOut · 11 ShiftOverShort · 12 Reversal.

### 10.5 PaymentMethod
1 Cash (نقدي) · 2 Card (بطاقة) · 3 BankTransfer (تحويل بنكي).

---

## 11. المعاملات والتزامن والأداء

- **مستوى العزل:** `READ COMMITTED` مع تفعيل `READ_COMMITTED_SNAPSHOT ON` على القاعدة (القراءات لا تنتظر الكتابة — مهم لشاشات التقارير أثناء البيع). الكتابات الحرجة تستخدم أقفالاً صريحة (`UPDLOCK`) والتحديثات الذرية المشروطة.
- **ترتيب الأقفال الثابت داخل الاعتماد:** DocumentSequences ← Products (تصاعدي بالمعرف) ← StockBalances ← CashAccounts ← قيود الطرف. يمنع الـ Deadlock بين المستندات المتزامنة.
- **استراتيجية إعادة المحاولة:** `EnableRetryOnFailure` مع تغليف المعاملة اليدوية بـ `CreateExecutionStrategy().ExecuteAsync(...)`؛ إعادة المحاولة آمنة بفضل `ClientRequestId`.
- **التجميعات:** التقارير تستخدم استعلامات تجميع على الخادم + فهارس تغطية؛ يُسمح بـ Views SQL في الترحيلات للتقارير الثقيلة (توثق هنا).
- **أهداف الحجم:** 10k صنف، 2k عميل، 500k فاتورة بيع (~5 أسطر) — تتحقق في Phase 17.
- **الأرشفة:** لا أرشفة في 1.0؛ البيانات تبقى. سجل التدقيق يمكن تقليمه بعد سنتين بإجراء إداري موثق (مؤجل).

## 12. البذور الأولية (Seed)
الوحدات، التصنيف «عام»، المخزن الرئيسي (افتراضي)، الزبون النقدي (`IsWalkIn`)، الحسابات النقدية (الصندوق الرئيسي، الحساب البنكي)، أسباب التسوية والمرتجع والسندات، تصنيفات المصروفات، تسلسلات المستندات بالبادئات، الأدوار الأربعة وصلاحياتها، الإعدادات الافتراضية. **لا مستخدمين** (معالج التهيئة — D-017).

## 13. سجل التغييرات على هذا التصميم
| التاريخ | التغيير | السبب | المرحلة |
|---|---|---|---|
| 2026-11-07 | الإصدار الأول | Phase 0 | P0 |
| 2026-11-07 | توضيح أن الترتيب العربي لا يتجاهل التشكيل (§1) | اختبار تكامل في P1 — D-041 | P1 |
| 2026-11-07 | سياسة الترحيل التلقائي (§14) | طلب المالك — D-043 | — |
| 2026-11-07 | الترتيب `Arabic_100_CI_AI` يُضبط في `CREATE DATABASE` داخل المُهيئ، لا عبر `UseCollation` في النموذج | `UseCollation` يولّد `ALTER DATABASE` داخل الترحيل (ممنوع — §14.3)؛ المُهيئ يحذّر إن وجد قاعدة بترتيب آخر | P2 |
| 2026-11-07 | البذور عبر `DatabaseSeeder` صريح (يضيف الناقص فقط) يستدعيه المُهيئ، لا `UseAsyncSeeding` | نفس الكود هو «مزامنة البذور» بعد كل ترحيل، ويُختبر مباشرة | P2 |
| 2026-11-07 | صنف الصلاحية اسمه `RolePermissionGrant` (الجدول `RolePermissions` بلا تغيير) | قاعدة المحلل CA1711 تمنع اللاحقة Permission | P2 |
| 2026-11-07 | `InventoryCounts.Status` يستخدم `DocumentStatus` (Draft=مفتوح، Posted=معتمد، Voided=ملغى) + عمود `IsSubmitted`؛ أُلغي `ApprovedAtUtc/ApprovedByUserId` لصالح `PostedAtUtc/PostedByUserId` | توحيد دورة حياة المستندات وقيود CHECK | P2 |
| 2026-11-07 | `ReturnSettlementMethod`: CashRefund=1، PartyCredit=2 (مشترك للبيع والشراء) | تعداد واحد بدل CustomerCredit/SupplierCredit | P2 |
| 2026-11-07 | `VoucherReasons.Scope` بدل `VoucherType`؛ `CashTransactions.PartyType` غير فارغ (None=0) | وضوح الاسم؛ قيد CHECK واحد للطرف | P2 |
| 2026-11-07 | فهرس البنود المفتوحة صار `(CustomerId|SupplierId, IsReversed, DueDate)` بدل الفهرس المرشّح `SettledAmount < Debit + Credit` | الفهارس المرشحة في SQL Server لا تقارن عمودين | P2 |
| 2026-11-07 | أعمدة الإنشاء في السجلات الإلحاقية عبر `LedgerRecord`/`IHasCreationStamp`؛ كل رأس مستند له فهرس `(Status, DocumentDate)`؛ EF ينشئ فهارس لمفاتيح التدقيق الخارجية | فهارس قوائم المستندات؛ سلوك EF الافتراضي مقبول لحجم المحل | P2 |
| 2026-11-07 | `Customers.Code`/`Suppliers.Code` nvarchar(20)؛ كود الزبون النقدي `WALKIN`؛ كود المخزن الرئيسي `MAIN` | — | P2 |

## 14. سياسة الترحيلات (D-043 — ملزمة)

### 14.1 كيف تُطبق
تلقائياً عند إقلاع البرنامج داخل شاشة البداية، دون تدخل المستخدم:
`sp_getapplock('Mizad.Migrations')` ⇐ إعادة فحص المعلق ⇐ نسخة `PreMigration` (COPY_ONLY + CHECKSUM + VERIFYONLY) ⇐ `MigrateAsync` (كل ترحيل في معاملة) ⇐ مزامنة البذور ⇐ تحقق ⇐ تحرير القفل ⇐ تدقيق.
فشل النسخة ⇐ لا ترحيل. فشل ترحيل ⇐ تراجعه وحده، والقاعدة على آخر ترحيل سليم، والنسخة متاحة.

### 14.2 كيف يضيف المطور تغييراً على المخطط
1. عدّل الكيان وإعداده (`IEntityTypeConfiguration`).
2. `dotnet ef migrations add <PascalCaseName> --project src/Mizad.Infrastructure --startup-project src/Mizad.Infrastructure --output-dir Persistence/Migrations`
3. راجع ملف الترحيل والـ SQL: `dotnet ef migrations script <Previous> <New> --project src/Mizad.Infrastructure --startup-project src/Mizad.Infrastructure`
4. إن أضاف صلاحية/إعداداً/قيمة مرجعية: حدّث مزامنة البذور.
5. شغّل الاختبارات: `HasPendingModelChanges`، تطبيق من الصفر، حراسة العمليات المدمرة، اختبارات الترقية.
6. سجّل التغيير في §13.

### 14.3 قواعد
- لا `EnsureCreated`؛ لا تعديل لترحيل مدموج/مطبق؛ لا تغيير يدوي للمخطط في SQL Server؛ لا حذف لملفات الترحيل أو لقطة النموذج.
- العمليات المدمرة (`DropTable`, `DropColumn`, `RenameColumn`, `RenameTable`, تضييق نوع عمود) تتطلب موافقة المالك، وترحيل البيانات داخل نفس الترحيل، وإدراجاً في قائمة الاستثناء أدناه — وإلا يفشل اختبار الحراسة.
- إضافة عمود `NOT NULL` لجدول قائم تتطلب قيمة افتراضية أو ترحيل بيانات.
- لا `ALTER DATABASE` ولا عمليات خارج المعاملة داخل الترحيلات (مكانها `DatabaseInitializer` عند الإنشاء فقط).
- كل إصدار منشور تُحفظ لقطة مخططه (سكربت) في `tests/Mizad.IntegrationTests/Baselines/vX.Y.Z.sql` لاختبار الترقية منه.

### 14.4 قائمة الاستثناء للترحيلات المدمرة المعتمدة
| الترحيل | العملية | الموافقة | ترحيل البيانات |
|---|---|---|---|
| — | — | — | — |

### 14.5 الصلاحيات المطلوبة في SQL Server
| العملية | الصلاحية |
|---|---|
| تشغيل البرنامج + الترحيل التلقائي + النسخ | `db_owner` على قاعدة مِيزاد فقط |
| إنشاء القاعدة لأول مرة (معالج الإعداد) | `dbcreator` (أو مصادقة Windows لمدير الجهاز) |
| الاستعادة | `sysadmin` أو `dbcreator` + ملكية القاعدة (Phase 16) |

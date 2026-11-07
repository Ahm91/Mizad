# agent.md — الخطة الرسمية لتطوير «مِيزاد»

> **المصدر الرسمي لخطة التطوير وحالة التنفيذ.** يُقرأ مع `AGENT_INSTRUCTIONS.md` (إلزامي أولاً) و`PROGRESS.md` و`DECISIONS.md`.
> حدّث مربع المهمة فور التحقق من نجاحها. لا تكتب `Completed` لمرحلة دون أدلة تحقق.
> الحالات المسموحة: `Not Started` · `In Progress` · `Blocked` · `Completed`.

| البند | القيمة |
|---|---|
| آخر تحديث | 2026-11-07 |
| المرحلة الحالية | **Phase 3 — Authentication & Security Foundation** (Not Started) |
| آخر مرحلة مكتملة | Phase 2 — Database Design & Infrastructure (2026-11-07) |
| المهمة التالية | **3.1** كتالوج الصلاحيات `Permissions` |

---

## 1. معلومات المشروع الأساسية

| البند | القيمة |
|---|---|
| الاسم | **مِيزاد** — مِيزاد لإدارة المبيعات والمشتريات والمخزون |
| النوع | تطبيق Windows مكتبي تجاري، عربي بالكامل RTL، خط Tajawal |
| التقنيات | C# · WPF/XAML · .NET 10 LTS · SQL Server · EF Core 10 · MVVM (CommunityToolkit.Mvvm) · Generic Host DI · Serilog · xUnit v3 |
| المعمارية | Modular Monolith بطبقات نظيفة (D-001) |
| التشغيل | محلي دون إنترنت؛ جهاز واحد أو شبكة محلية بقاعدة SQL Server مشتركة (D-019) |
| بيئة التطوير المؤكدة | Windows 11، .NET SDK 10.0.401، SQL Server 2025 Developer (`.`)، Tajawal مثبت، VS 2026 (`Mizad.slnx`) |
| الحالة الحالية للكود | Phase 1 + Phase 2: قاعدة بيانات كاملة (45 جدولاً) بترحيل تلقائي آمن، خدمات البنية، 85 اختباراً ناجحاً؛ لا واجهات بعد (Phase 4) |
| المراجع البصرية | `UI/` (24 صورة) + `Logo.png` — الخريطة في [docs/UI_IMPLEMENTATION_MAP.md](docs/UI_IMPLEMENTATION_MAP.md) |
| Git | مستودع موجود أنشأه المالك من Visual Studio: الفرع `master`، البعيد `origin` = `https://github.com/Ahm91/Mizad.git`، التزامان أوليان (`.gitignore`/`.gitattributes` القياسيان + ملفات القالب و`UI/` و`Logo.png`). وثائق Phase 0 غير ملتزم بها بعد. |

### ملفات المشروع المرجعية
| الملف | الغرض |
|---|---|
| [AGENT_INSTRUCTIONS.md](AGENT_INSTRUCTIONS.md) | قواعد العمل الإلزامية لكل الوكلاء |
| [PROGRESS.md](PROGRESS.md) | آخر حالة فعلية وتعليمات الاستكمال |
| [DECISIONS.md](DECISIONS.md) | سجل القرارات D-001… |
| [docs/REQUIREMENTS.md](docs/REQUIREMENTS.md) | المتطلبات FR/NFR وحالات الاستخدام |
| [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) | الطبقات والأنماط والمجلدات |
| [docs/DATABASE_DESIGN.md](docs/DATABASE_DESIGN.md) | الجداول والحقول والقيود والفهارس |
| [docs/BUSINESS_RULES.md](docs/BUSINESS_RULES.md) | القواعد BR-xxx والصلاحيات والإعدادات |
| [docs/UI_IMPLEMENTATION_MAP.md](docs/UI_IMPLEMENTATION_MAP.md) | الصور ← الشاشات، رموز التصميم، التعديلات DV-xx |
| [docs/TEST_PLAN.md](docs/TEST_PLAN.md) | سيناريوهات TS-xxx وسجل التنفيذ |
| [docs/DEPLOYMENT.md](docs/DEPLOYMENT.md) | البيئة، التثبيت، التحديث، الدعم |

---

## 2. الهدف والنطاق الوظيفي

**الهدف:** منتج تجاري حقيقي لإدارة محل/مؤسسة صغيرة أو متوسطة: بيع سريع بالباركود، مشتريات، مخزون متعدد المخازن بتكلفة متوسطة، عملاء وموردون وديون، صندوق ومصروفات، تقارير قابلة للمطابقة، صلاحيات، نسخ احتياطي، طباعة، ومثبت.

**الأولويات بالترتيب:** صحة العمليات المالية والمخزنية ← الاستقرار والأمان ← السرعة وسهولة الاستخدام ← المطابقة البصرية ← الإضافات.

**النطاق:** محل واحد، عملة واحدة، عدة مخازن، عدة أجهزة في شبكة محلية، بلا إنترنت.

---

## 3. الوظائف الأساسية المعتمدة (الإصدار 1.0)

التفصيل والأولويات في [docs/REQUIREMENTS.md](docs/REQUIREMENTS.md) §4.

1. الدخول، القفل، معالج التهيئة، المستخدمون، الأدوار، الصلاحيات الخاصة، سجل التدقيق.
2. الأصناف: تصنيفات، وحدات متعددة بمعاملات، باركود متعدد، سعر مفرد وجملة، صور اختيارية، استيراد Excel.
3. المخزون: محرك حركات، متوسط مرجح متحرك، منع السالب والتزامن، عدة مخازن، تحويل، تسويات، جرد بلقطة، نواقص وراكد.
4. المشتريات: فواتير بالخصم والتكلفة الواصلة والدفع الجزئي، مسودات، إلغاء بالعكس.
5. المبيعات ونقطة البيع: باركود، وحدات، خصومات بحدود، نقدي/آجل/جزئي، نقد/بطاقة/تحويل، مسودات معلقة، إلغاء بالعكس، اختصارات.
6. المرتجعات: بيع وشراء مرتبطة بالفاتورة، حدود الكمية، إعادة للمخزون أو تالف، رد نقدي أو رصيد.
7. الذمم: عملاء وموردون، أرصدة افتتاحية، تحصيل وتسديد بمطابقة FIFO أو صريحة، كشوف حساب، أعمار ديون.
8. الخزينة: حسابات نقد وبنك، سندات قبض وصرف، مصروفات بتصنيفات، تحويلات، ورديات اختيارية.
9. لوحة التحكم و15 تقريراً قابلاً للمطابقة، بحث عام، تنبيهات.
10. طباعة A4 وحرارية 80/58، معاينة، PDF، Excel.
11. الإعدادات، النسخ اليدوي والمجدول، الاستعادة المحمية، الترحيل الآمن.
12. مثبت عربي بوضعي خادم/عميل.

## 4. الوظائف المؤجلة أو المستبعدة

| الوظيفة | الحالة | المرجع |
|---|---|---|
| تعدد العملات | مستبعد 1.0 | D-039 |
| الفوترة الإلكترونية الحكومية (ZATCA وغيرها) | مستبعد 1.0 | D-039 |
| تعدد الفروع والمزامنة عبر الإنترنت | مستبعد 1.0 | D-039 |
| محاسبة عامة بقيد مزدوج ودليل حسابات | مستبعد 1.0 | D-039 |
| الرواتب وشؤون الموظفين | مستبعد | D-039 |
| العروض والولاء والكوبونات | مستبعد 1.0 | D-039 |
| الأرقام التسلسلية والدفعات وتواريخ الصلاحية | مستبعد 1.0 | D-039 |
| باركود الميزان بالوزن المضمن | مؤجل (1.1) | D-039 |
| أوامر الشراء وعروض الأسعار | مؤجل (1.1) | D-039 |
| فتح درج النقد ESC/POS | مؤجل (1.1) | D-039 |
| استيراد العملاء/الموردين وأرصدتهم من Excel | مؤجل (1.1) | — |
| قفل الشاشة بعد الخمول | اختياري (C) | FR-ID-09 |
| الترخيص والتفعيل | مؤجل لقرار المالك | D-034 |
| نجمة «مفضل» للمورد | مستبعد | DV-20 |
| أرشفة البيانات القديمة | مؤجل | DATABASE_DESIGN §11 |

**أي إضافة من هذا الجدول تتطلب موافقة مالك المشروع وقراراً جديداً في DECISIONS.md.**

---

## 5. البنية المعمارية (ملخص — التفصيل في [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md))

```
Mizad.slnx
├── src/
│   ├── Mizad.Domain          (net10.0)          كيانات، تعدادات، حاسبات نقية، Result/Error
│   ├── Mizad.Application     (net10.0)          خدمات حالات الاستخدام، DTOs، مدققات، تفويض، واجهات
│   ├── Mizad.Infrastructure  (net10.0-windows)  EF Core/SQL Server، ترحيلات، المحرك الذري، النسخ، التشفير، PDF/Excel
│   └── Mizad.Desktop         (net10.0-windows)  WPF: Views/ViewModels/Themes/Controls، جذر التركيب
├── tests/
│   ├── Mizad.UnitTests
│   └── Mizad.IntegrationTests (SQL Server حقيقي)
├── UI/  docs/  Logo.png
├── agent.md  AGENT_INSTRUCTIONS.md  PROGRESS.md  DECISIONS.md  CLAUDE.md
└── Directory.Build.props  Directory.Packages.props  .editorconfig  .gitignore
```
- الاعتماد: Domain ← Application ← Infrastructure؛ Desktop يستخدم Application ويسجل Infrastructure في DI فقط.
- `Mizad.Shared` المقترح أُلغي بقرار D-001 (لا محتوى يبرره).
- DbContext لكل حالة استخدام عبر `IDbContextFactory` (D-004). لا منطق أعمال في Code-Behind.

## 6. تصميم قاعدة البيانات (ملخص — التفصيل في [docs/DATABASE_DESIGN.md](docs/DATABASE_DESIGN.md))

- ~45 جدولاً في 9 مجموعات: الأمان، الإدارة، الأصناف، المخزون، الأطراف، المبيعات، المشتريات، المرتجعات، الخزينة.
- **مصادر الحقيقة الإلحاقية:** `StockMovements`، `PartyLedgerEntries` (+`PartySettlements`)، `CashTransactions`.
- **المُجسَّد الوحيد:** `StockBalances` (يكتبه المحرك فقط بتحديث ذري — D-006)، و`SettledAmount`/`ReturnedBaseQuantity` كذواكر مؤقتة بثوابت مختبرة.
- الأنواع: Money `decimal(19,4)`، UnitCost `decimal(19,6)`، Qty `decimal(18,3)`. الترتيب `Arabic_100_CI_AI`، RCSI مفعّل.
- كل مستند: `Draft/Posted/Voided`، رقم عند الاعتماد، `ClientRequestId` فريد، `RowVersion`.

## 7. خريطة الواجهات والصور (ملخص — التفصيل في [docs/UI_IMPLEMENTATION_MAP.md](docs/UI_IMPLEMENTATION_MAP.md))

| الصورة | الشاشة | المرحلة |
|---|---|---|
| UI-01 | تسجيل الدخول | P4 |
| UI-02 | لوحة التحكم + الإطار (الشريط والترويسة) | P4 (الإطار)، P14 (اللوحة) |
| UI-03 | نقطة البيع — فاتورة بيع جديدة | P9 |
| UI-04 | فواتير المبيعات (+ نمط فواتير المشتريات) | P9 (P8) |
| UI-05 | فاتورة شراء | P8 |
| UI-06 | الأصناف | P5 |
| UI-07 | المخزون والجرد | P7، P13 |
| UI-08 | العملاء والديون | P6، P11 |
| UI-09 | الموردون | P6، P11 |
| UI-10 | الصندوق والمصروفات | P12 |
| UI-11 | المستخدمون والصلاحيات | P16 |
| UI-12 | الإعدادات والنسخ الاحتياطي | P16 (+P15 للطباعة) |
| UI-13…UI-17 | المرتجعات (UI-14 أساسي) | P10 |
| UI-18…UI-24 | مركز التقارير (UI-19 أساسي) | P14 |
| Logo.png | الهوية والأيقونة | P4 |

رموز التصميم المستخرجة: كحلي `#0C223D`، فيروزي أساسي `#019B92`، خلفية `#ECF2F9`، Tajawal. تعديلات التصميم المبررة DV-01…DV-22.

## 8. خريطة الخدمات والطبقات

| الوحدة | Domain | Application | Infrastructure | Desktop (Views) | المراحل |
|---|---|---|---|---|---|
| Identity | User, Role, Permissions | AuthService, UserService, RoleService, AuthorizationService, SetupService | Pbkdf2PasswordHasher, AuditLogger | Login, SetupWizard, ChangePassword, Users | P3, P4, P16 |
| Administration | Setting, DocumentSequence, BackupRecord | SettingsService, BackupService, RestoreService, DatabaseInitializer (ترحيل تلقائي D-043), DiagnosticsService, IConnectionMonitor, IBusyTracker, IRecoveryStore | DatabaseBackupService, MigrationLock, Migrations, ConnectionMonitor, ConnectionSettingsStore (DPAPI) | SplashWindow + StartupOrchestrator, ConnectionSetup (معالج الإعداد), Settings, BackupHistory, AuditLog, مؤشر الاتصال | P2, P4, P16 |
| Catalog | Product, ProductUnit, Barcode, Category, Unit; Ean13, ArabicNormalizer | ProductService, ProductLookupService, CategoryService, UnitService, ProductImportService | ClosedXmlImporter, ImageResizer | Products, Categories/Units dialogs, Import | P5 |
| Inventory | StockMovement, StockBalance, Adjustment, Count, Transfer; AverageCostCalculator | IInventoryEngine, StockQueryService, AdjustmentService, InventoryCountService, StockTransferService | InventoryEngine (SQL ذري) | Inventory, InventoryCount, Transfer | P7, P13 |
| Parties | Customer, Supplier, OpeningBalance, LedgerEntry, Settlement; FifoSettlementCalculator | CustomerService, SupplierService, PartyLedger, SettlementEngine, StatementService, OpeningBalanceService | — | Customers, Suppliers, PartyVoucher, OpeningBalance | P6, P11 |
| Sales | SalesInvoice(+Lines); InvoiceCalculator | SalesInvoiceService, SalesQueryService | — | SalesPos, SalesInvoices, SalesInvoiceDetails | P9 |
| Purchasing | PurchaseInvoice(+Lines) | PurchaseInvoiceService, PurchaseQueryService | — | PurchaseInvoice, PurchaseInvoices | P8 |
| Returns | SalesReturn, PurchaseReturn; ReturnCalculator | SalesReturnService, PurchaseReturnService | — | Returns | P10 |
| Treasury | CashAccount, CashTransaction, CashVoucher, Expense, CashTransfer, Shift | CashLedger, VoucherService, ExpenseService, CashTransferService, ShiftService, CashQueryService | — | Cashbox, VoucherPanel, Expense, Shift | P8 (النواة), P11, P12 |
| Reporting | — | DashboardService, ReportService (R01–R15), GlobalSearchService, NotificationService | استعلامات/Views SQL | Dashboard, Reports, GlobalSearch, Notifications | P14 |
| Documents | DocumentModel | DocumentModelBuilders, IPrintService, IExcelExporter | QuestPdfRenderer, ClosedXmlExporter | PrintService (طباعة WPF — Desktop لأن Infrastructure ممنوع عليه WPF)، PrintPreview | P15 |

---

## 9. خطة مراحل التطوير — نظرة عامة وحالة كل مرحلة

| المرحلة | الاسم | الحالة | تعتمد على | الشاشات |
|---|---|---|---|---|
| P0 | Requirements Analysis & Master Planning | **Completed** (2026-11-07) | — | كل الصور |
| P1 | Solution Architecture & Project Foundation | **Completed** (2026-11-07) | P0 | — |
| P2 | Database Design & Infrastructure | **Completed** (2026-11-07) | P1 | — |
| P3 | Authentication & Security Foundation | Not Started | P2 | — |
| P4 | Design System & Application Shell | Not Started | P3 | UI-01, UI-02 (الإطار) |
| P5 | Products & Categories Management | Not Started | P4 | UI-06 |
| P6 | Customers & Suppliers Management | Not Started | P4 | UI-08, UI-09 |
| P7 | Inventory Core & Stock Movement Engine | Not Started | P5 | UI-07 |
| P8 | Purchases Management (+ نواة الخزينة) | Not Started | P6, P7 | UI-05, UI-04 |
| P9 | Sales & POS Management | Not Started | P6, P7, P8 (نواة الخزينة) | UI-03, UI-04 |
| P10 | Sales & Purchases Returns | Not Started | P8, P9 | UI-13…UI-17 |
| P11 | Receivables & Payables | Not Started | P8, P9 | UI-08, UI-09, UI-10 |
| P12 | Cashbox & Expenses | Not Started | P11 | UI-10 |
| P13 | Inventory Counting & Adjustments | Not Started | P7 | UI-07 |
| P14 | Dashboard & Reports | Not Started | P9–P13 | UI-02, UI-18…UI-24 |
| P15 | Printing & Exporting | Not Started | P9 (يبدأ)، P14 (يكتمل) | UI-12 (قسم الطباعة) |
| P16 | Administration, Settings & Backup | Not Started | P4 (يبدأ)، P15 | UI-11, UI-12 |
| P17 | Integration Testing & Quality Assurance | Not Started | P1–P16 | الكل |
| P18 | Packaging, Deployment & Release | Not Started | P17 | — |

### الاعتماديات وفرص التوازي
```
P0 → P1 → P2 → P3 → P4 ─┬→ P5 → P7 ─┬→ P13 ───────────────┐
                        └→ P6 ───────┴→ P8 → P9 → P10      │
                                              └→ P11 → P12 ─┴→ P14 → P15 → P16 → P17 → P18
P16 (المستخدمون/الإعدادات/النسخ) يمكن أن تبدأ بالتوازي بعد P4، لكن معاينة الطباعة تكتمل بعد P15. (نسخة PreMigration والترحيل التلقائي أصبحا في P2 — D-043.)
```
- **متوازٍ آمن:** P5 ∥ P6 (بعد P4)؛ P13 ∥ P8–P12 (بعد P7)؛ أجزاء P16 (16.1–16.8) ∥ P8–P14؛ قوالب P15 لكل مستند فور اكتمال مرحلته.
- **حرج:** P7 (المحرك) و8.1–8.2 (نواة الخزينة) و6.3–6.4 (الدفتر والمطابقة) تسبق أي اعتماد مستند.
- **قاعدة:** لا تبدأ مهمة قبل اكتمال المهام المذكورة في «Dependencies» الخاصة بها.

---

## 10–13. تفاصيل المراحل: المهام الفرعية · الاعتماديات · معايير القبول · الاختبارات

> كل مرحلة أدناه تحتوي البنود 10 (Tasks)، 11 (Dependencies — مع الرسم في §9)، 12 (Acceptance Criteria)، 13 (Testing) من الهيكل الإلزامي، إضافة للمخاطر والأدلة.
> ترقيم المهام `P.N` (مثل 9.4) ثابت — لا تُعد ترقيم المهام؛ أضف مهاماً جديدة بأرقام لاحقة (9.12…) أو فرعية (9.4a).

## Phase 0 — Requirements Analysis & Master Planning

Status: Completed (2026-11-07)

### Objective
تحليل المتطلبات والصور، تصميم المعمارية وقاعدة البيانات وقواعد الأعمال، وإنشاء ملفات إدارة المشروع والوثائق.

### Dependencies
لا شيء.

### Tasks
- [x] 0.1 فحص بنية المشروع والبيئة (SDK 10.0.401، SQL Server 2025 Developer، Tajawal، قالب WPF يبني بنجاح؛ مستودع Git أنشأه المالك أثناء الجلسة على `master` مع `origin` على GitHub).
- [x] 0.2 فحص الصور الـ24 والشعار بصرياً وتحديد الشاشة لكل صورة (جيلان من التصاميم).
- [x] 0.3 استخراج رموز التصميم بأخذ عينات بكسل (الألوان) وتحديد الخطوط والمسافات والمكونات المشتركة.
- [x] 0.4 تحليل المتطلبات وحالات الاستخدام والرحلات ⇐ `docs/REQUIREMENTS.md`.
- [x] 0.5 قواعد الأعمال والمحاسبة والمخزون والصلاحيات والإعدادات ⇐ `docs/BUSINESS_RULES.md`.
- [x] 0.6 المعمارية ⇐ `docs/ARCHITECTURE.md`.
- [x] 0.7 تصميم قاعدة البيانات ⇐ `docs/DATABASE_DESIGN.md`.
- [x] 0.8 خريطة الواجهات والتعديلات المبررة ⇐ `docs/UI_IMPLEMENTATION_MAP.md`.
- [x] 0.9 خطة الاختبارات ⇐ `docs/TEST_PLAN.md`.
- [x] 0.10 خطة النشر ⇐ `docs/DEPLOYMENT.md`.
- [x] 0.11 سجل القرارات D-001…D-042 ⇐ `DECISIONS.md`.
- [x] 0.12 الخطة الشاملة ⇐ `agent.md` (هذا الملف).
- [x] 0.13 `AGENT_INSTRUCTIONS.md` و`PROGRESS.md` و`CLAUDE.md`.
- [x] 0.14 مراجعة الخطة للفجوات والاعتماديات (§10 مراجعة الفجوات أدناه).

### Related UI
UI-01…UI-24، Logo.png.

### Implementation
وثائق فقط — لا كود.

### Database Changes
لا شيء (التصميم موثق).

### Testing
لا اختبارات آلية (لا كود). خط أساس: `dotnet build Mizad.slnx` على القالب ⇒ نجاح، 0 تحذيرات، 0 أخطاء.

### Acceptance Criteria
- [x] كل الصور مفحوصة ومرتبطة بشاشة ومرحلة.
- [x] كل قاعدة من القواعد الحرجة الـ18 مرتبطة بقاعدة تفصيلية واختبار.
- [x] كل مرحلة لها مهام واعتماديات ومعايير قبول واختبارات ومخاطر.
- [x] الوثائق السبع وملفات الإدارة الأربعة موجودة بمحتوى مفيد.

### Risks
القرارات «المقترحة» (العملة، الضريبة، المخزون السالب، ترخيص QuestPDF، المثبت) تحتاج تأكيد المالك — مدرجة في PROGRESS.md.

### Completion Evidence
- ملفات منشأة: `agent.md`, `AGENT_INSTRUCTIONS.md`, `PROGRESS.md`, `DECISIONS.md`, `CLAUDE.md`, `docs/REQUIREMENTS.md`, `docs/ARCHITECTURE.md`, `docs/DATABASE_DESIGN.md`, `docs/BUSINESS_RULES.md`, `docs/UI_IMPLEMENTATION_MAP.md`, `docs/TEST_PLAN.md`, `docs/DEPLOYMENT.md`.
- لم يُعدل أي ملف كود أو صورة.
- البناء: `dotnet build Mizad.slnx` ⇒ Build succeeded, 0 Warning(s), 0 Error(s).

### Completion Summary
اكتمل التحليل والتخطيط. 42 قراراً، ~45 جدولاً مصمماً، 32 شاشة مخططة، 22 تعديلاً تصميمياً مبرراً، 19 مرحلة بمهام تنفيذية. المرحلة التالية P1.

### مراجعة الفجوات (المهمة 0.14)
| الفجوة المكتشفة | المعالجة |
|---|---|
| الدفع عند البيع/الشراء يحتاج الخزينة قبل P11/P12 | نواة الخزينة نُقلت إلى 8.1–8.2 |
| الرصيد الافتتاحي للصنف في P5 يحتاج محرك P7 | الحقل يُفعّل في المهمة 7.8 |
| نسخة PreMigration (P2) تحتاج خدمة النسخ (P16) | ~~المهمة 16.9 تربطها~~ ⇐ **محدّث 2026-11-07 (D-043):** خدمة نسخ أساسية تُبنى في 2.22 والترحيل التلقائي في 2.21–2.26؛ Phase 16 توسعها فقط |
| أزرار الطباعة في P8–P13 تسبق P15 | تظهر معطلة بتلميح «متاحة لاحقاً» حتى المهمة 15.7 |
| التقارير تحتاج بيانات كبيرة للتطوير | مولد بيانات تجريبية (14.1) — في Debug فقط وقاعدة منفصلة |
| تصاميم غير موجودة (الجرد، التحويل، الإعداد، المعاينة…) | شاشات مشتقة S06–S32 بنفس نظام التصميم |
| تسوية الفروقات النقدية للورديات والجرد في الربح | معرّفة في BR-066، BR-080 |
| الشعار نسخة نقطية واحدة بنص كحلي | المهمة 4.4 + الخطر R-14 |

---

## Phase 1 — Solution Architecture & Project Foundation

Status: Completed (2026-11-07)

### Objective
إنشاء الحل والمشاريع والطبقات وأساس الاستضافة وDI والتسجيل والتكوين والاختبارات، بحيث يبني ويختبر ويُقلع.

### Dependencies
P0.

### Tasks
- [x] 1.1 استكمال إعداد Git: المستودع موجود (`master` + `origin`) — **لا `git init`**. أضف إلى `.gitignore` الموجود (قالب VisualStudio) استثناءات مِيزاد: `artifacts/`، `*.bak`، `connection.json`، `machine.json`، `user.json`، `logs/`؛ تأكد أن `.gitattributes` يعامل الصور `binary`؛ أنشئ الفرع `phase/01-foundation` من `master` (D-040). الالتزام والدفع فقط بموافقة المالك (AGENT_INSTRUCTIONS §6). — ✅ الفرع `phase/01-foundation` أُنشئ؛ لا التزامات.
- [x] 1.2 `.editorconfig` (Namespaces بنطاق الملف، قواعد التسمية، `var`، ترتيب using، UTF-8 مع BOM لملفات XAML/CS). — ✅ مع تعطيلات موثقة بسببها: CA1303، CA1716، CA1848/CA1873، CA2007؛ وفي الاختبارات CA1707/CA1515/CA1062.
- [x] 1.3 نقل القالب: `Mizad/` ⇐ `src/Mizad.Desktop/`، إعادة تسمية `Mizad.Desktop.csproj`، `RootNamespace=Mizad.Desktop`، `AssemblyName=Mizad`، تحديث `x:Class` والـ namespaces، حذف `Mizad.csproj.user` و`bin/obj` القديمة، تحديث `Mizad.slnx`. — ✅ بالنسخ ثم الحذف لأن Visual Studio كان يقفل المجلد؛ المجلد القديم أُزيل كاملاً.
- [x] 1.4 إنشاء `src/Mizad.Domain` (net10.0)، `src/Mizad.Application` (net10.0)، `src/Mizad.Infrastructure` (net10.0-windows)، وإضافتها إلى `Mizad.slnx` بمجلدي `src` و`tests`.
- [x] 1.5 `Directory.Build.props`: `Nullable`, `ImplicitUsings`, `LangVersion=latest`, `TreatWarningsAsErrors=true` (src)، `AnalysisLevel=latest-recommended`، `Version=0.1.0`، `Product=مِيزاد`، `Company`، `Deterministic`. — ✅ `TreatWarningsAsErrors` مطبق على **كل** المشاريع بما فيها الاختبارات (أشد من المخطط)، + `EnforceCodeStyleInBuild`.
- [x] 1.6 `Directory.Packages.props` (Central Package Management) بإصدارات مثبتة؛ أعد التحقق من أحدث إصدار مستقر وحدّث جدول D-003 إن تغير. — ✅ مع `CentralPackageTransitivePinningEnabled`؛ `Microsoft.Data.SqlClient` مثبت على 6.1.8 (توافق EF Core 10)؛ D-003 محدّث.
- [x] 1.7 مراجع المشاريع وفق ARCHITECTURE §1.
- [x] 1.8 `Mizad.Domain/Common`: `Entity<TId>`، `AuditableEntity`، `IHasRowVersion`، `Result`/`Result<T>`، `Error(Code, Message)`، `DomainException`، `Guard`. — ✅ + `ErrorKind` و`IAuditable`.
- [x] 1.9 `Mizad.Application/Abstractions`: `IClock`، `ICurrentUser`، `IAuthorizationService`، `IAuditLogger`، `IUnitOfWork` (تواقيع)، و`AddApplication()`. — ✅ + واجهات الإعدادات المحلية (`IConnectionSettingsStore`، `IMachineSettingsStore`، `IUserPreferencesStore`، `IConnectionStringFactory`). **`IUnitOfWork` أُجّل عمداً إلى 2.15** لأن توقيعه يعتمد على `IMizadDbContext` (يُنشأ في 2.1)، وتعريفه الآن يعني تغييره لاحقاً.
- [x] 1.10 `Mizad.Infrastructure`: `AddInfrastructure(IConfiguration)`؛ Serilog (ملف دوار في `%LocalAppData%\Mizad\logs`، 30 يوماً، Enrichers: Machine/User/Version، حجب كلمات المرور وسلاسل الاتصال)؛ `ConnectionSettingsStore` (DPAPI LocalMachine)، `MachineSettingsStore`، `UserPreferencesStore` (D-042). — ✅ التوقيع الفعلي `AddInfrastructure(MizadPaths)` (لا حاجة لـ IConfiguration بعد)؛ إثراء User يُضاف عند الدخول في 3.5.
- [x] 1.11 `Mizad.Desktop`: Generic Host في `App.xaml.cs` (إزالة `StartupUri`)، `Composition/ServiceRegistration.cs`، معالجات الاستثناءات العامة الثلاثة مع رسالة عربية ورقم مرجعي، `app.manifest` (PerMonitorV2)، `MainWindow` مؤقتة عبر DI بـ `FlowDirection=RightToLeft`.
- [x] 1.12 `tests/Mizad.UnitTests` (xunit.v3 + Shouldly) + اختبارات معمارية تفحص المراجع (Domain بلا EF/WPF؛ Application بلا SqlServer/WPF/Infrastructure). — ✅ 6 اختبارات معمارية (مراجع IL + ملفات csproj + منع الإصدارات داخل csproj).
- [x] 1.13 `tests/Mizad.IntegrationTests` + `SqlServerFixture` (ينشئ/يحذف `Mizad_IT_<guid>` بالترتيب العربي، يقرأ `MIZAD_TEST_SQL`) + اختبار دخان للاتصال. — ✅ كشف أن الترتيب العربي لا يتجاهل التشكيل ⇐ D-041 محدّث.
- [x] 1.14 `scripts/build.ps1`: restore ⇒ build Release ⇒ test، مع ملخص وخروج بخطأ عند الفشل. — ✅ خيارات `-SkipTests` و`-UnitOnly`؛ ASCII خالص ليعمل على Windows PowerShell 5.1.
- [x] 1.15 التحقق: `dotnet build -c Release` بلا تحذيرات؛ `dotnet test` ناجح؛ التطبيق يُقلع ويكتب ملف سجل؛ تحديث agent.md وPROGRESS.md.

### Related UI
لا شيء.

### Implementation
`Mizad.slnx`، `Directory.Build.props`، `Directory.Packages.props`، `.editorconfig`، `.gitignore`، `.gitattributes`، `scripts/build.ps1`، المشاريع الستة، `App.xaml(.cs)`، `Composition/`، `app.manifest`.

### Database Changes
لا شيء (اختبار اتصال فقط).

### Testing
اختبارات المعمارية؛ اختبار `Result`؛ اختبار `ConnectionSettingsStore` (تشفير وفك كلمة مرور، عدم كتابتها نصاً)؛ اختبار دخان SQL.

### Acceptance Criteria
- البناء Release بلا تحذيرات ولا أخطاء.
- كل الاختبارات تمر (بما فيها التكامل على SQL المحلي).
- التطبيق يُقلع عبر Host ويغلق بلا استثناءات، وملف السجل يُنشأ.
- اختبار المعمارية يمنع الاعتمادات الممنوعة.
- لا أسرار في المستودع.

### Risks
- اختلاف أدوات VS 2026 مع `slnx` في سطر الأوامر — التحقق بـ `dotnet build Mizad.slnx`.
- `TreatWarningsAsErrors` قد يبطئ البداية — مقبول؛ لا تعطيل للتحذيرات دون توثيق.

### Completion Evidence
- **البناء:** `./scripts/build.ps1` ⇐ `dotnet build Mizad.slnx -c Release` ⇒ **0 Warning(s), 0 Error(s)**.
- **الاختبارات:** `dotnet test --solution Mizad.slnx -c Release` ⇒ **total 33، succeeded 33، failed 0، skipped 0** (وحدات 26: معمارية 6، Result 6، إعدادات محلية وDPAPI 7، سلسلة الاتصال 4، السجل 3؛ تكامل 7 على SQL Server 2025 المحلي).
- **التشغيل:** `Mizad.exe` (Release) أُقلع بالعنوان «مِيزاد — إدارة المبيعات والمشتريات» ويستجيب، أُغلق بـ `CloseMainWindow` ⇒ رمز خروج 0؛ السجل `%LocalAppData%\Mizad\logs\mizad-20261107.log` يحوي «Mizad 0.1.0 started…» و«Mizad exited with code 0».
- **النظافة:** `sys.databases LIKE 'Mizad_IT_%'` ⇒ 0 (القواعد المؤقتة تُحذف)؛ فحص الأسرار في `src/tests/scripts` ⇒ لا شيء.
- **ملفات جديدة:** `.editorconfig`، `global.json`، `Directory.Build.props`، `Directory.Packages.props`، `scripts/build.ps1`، `src/Mizad.Domain/**` (5 ملفات Common)، `src/Mizad.Application/**` (7)، `src/Mizad.Infrastructure/**` (10)، `src/Mizad.Desktop/{Composition/*, app.manifest, appsettings.json}`، `tests/Mizad.UnitTests/**` (7)، `tests/Mizad.IntegrationTests/**` (3).
- **ملفات معدلة:** `.gitignore`، `.gitattributes`، `Mizad.slnx`؛ `Mizad/*` ⇐ `src/Mizad.Desktop/*` (App.xaml(.cs)، MainWindow.xaml(.cs) أعيدت كتابتها، csproj أعيدت تسميته).
- **وثائق محدثة:** DECISIONS (D-003، D-041)، DATABASE_DESIGN §1، TEST_PLAN (الأوامر + السجل)، DEPLOYMENT (الأوامر)، ARCHITECTURE (§4.10).

### Completion Summary
الحل منظم في 4 مشاريع src + 2 اختبار بقواعد طبقات مُختبرة آلياً؛ إدارة مركزية للحزم؛ استضافة عامة في WPF مع Serilog ومعالجة أخطاء عامة عربية؛ إعدادات محلية بتشفير DPAPI لكلمة مرور SQL؛ اختبارات تكامل على SQL Server حقيقي بقاعدة مؤقتة بالترتيب العربي.
**ملاحظات للمراحل التالية:**
1. xunit.v3 v4 يعمل على **Microsoft.Testing.Platform** (مفعّل في `global.json`) ⇐ الأمر `dotnet test --solution Mizad.slnx` (لا `dotnet test Mizad.slnx`).
2. مشاريع `UseWPF` لا تستورد `System.IO` ضمنياً (أُضيف كـ `<Using>` في UnitTests).
3. داخل namespace `Mizad.Desktop` الاسم `Application` يشير إلى namespace `Mizad.Application` ⇐ استخدم `System.Windows.Application` صراحة.
4. **D-041 محدّث:** الترتيب العربي في SQL Server لا يتجاهل التشكيل ولا يوحّد الهمزات — البحث عبر `SearchName` حصراً (2.11).
5. التنفيذ الفعلي لطباعة WPF مكانه Desktop لا Infrastructure (اختبار المعمارية يمنع WPF في Infrastructure) — مُصحح في §8.

---

## Phase 2 — Database Design & Infrastructure

Status: Completed (2026-11-07)

### Objective
تنفيذ الكيانات والإعدادات والقيود والفهارس والترحيل الأول والبذور وخدمات البنية للمعاملات والتسلسلات وساعة الخادم والإعدادات.

### Dependencies
P1.

### Tasks
- [x] 2.1 `MizadDbContext` + `IMizadDbContext` + `ConfigureConventions` (decimal افتراضي 19,4، `datetime2` بدقة 3، طول نصوص صريح إلزامي)، `UseCollation("Arabic_100_CI_AI")`. — ✅ **انحراف موثق:** لا `UseCollation` في النموذج لأنه يولّد `ALTER DATABASE` داخل الترحيل (ممنوع — D-043)؛ الترتيب يُضبط في `CREATE DATABASE` داخل المُهيئ، والمُهيئ يحذّر إن وجد قاعدة بترتيب آخر.
- [x] 2.2 كيانات وإعدادات الأمان والإدارة: Users, Roles, RolePermissions, AuditLogs, Settings, DocumentSequences, BackupHistory.
- [x] 2.3 كيانات الأصناف: Categories, Units, Products, ProductUnits, ProductBarcodes, ProductImages.
- [x] 2.4 كيانات المخزون: Warehouses, StockBalances, StockMovements, AdjustmentReasons, StockAdjustments(+Lines), InventoryCounts(+Lines), StockTransfers(+Lines).
- [x] 2.5 كيانات الأطراف: Customers, Suppliers, OpeningBalances, PartyLedgerEntries, PartySettlements.
- [x] 2.6 كيانات المبيعات والمشتريات: SalesInvoices(+Lines), PurchaseInvoices(+Lines).
- [x] 2.7 كيانات المرتجعات: ReturnReasons, SalesReturns(+Lines), PurchaseReturns(+Lines).
- [x] 2.8 كيانات الخزينة: CashAccounts, CashTransactions, CashVouchers, VoucherReasons, ExpenseCategories, Expenses, CashTransfers, CashierShifts.
- [x] 2.9 كل قيود CHECK والفهارس الفريدة المُفلترة وRowVersion وسلوك الحذف Restrict كما في DATABASE_DESIGN.
- [x] 2.10 `AuditableEntityInterceptor` يملأ أعمدة التدقيق من `ICurrentUser`/`IClock`.
- [x] 2.11 `ArabicNormalizer` (Domain) + تعبئة `SearchName` تلقائياً عند الحفظ.
- [x] 2.12 البذور عبر `UseAsyncSeeding` (idempotent): الوحدات، «عام»، المخزن الرئيسي، الزبون النقدي، الحسابات النقدية، أسباب التسوية والمرتجع والسندات، تصنيفات المصروفات، التسلسلات، الإعدادات الافتراضية (الأدوار والصلاحيات في 3.2). لا مستخدمين. — ✅ **انحراف موثق:** بذّار صريح `DatabaseSeeder` (ساكن) يستدعيه المُهيئ بعد الترحيل، لا `UseAsyncSeeding`، ليكون هو نفسه «مزامنة البذور» (2.24) ويُختبر مباشرة.
- [x] 2.13 الترحيل `InitialCreate` ومراجعة SQL المولد (سكربت idempotent في `artifacts/sql/`).
- [x] 2.14 `DatabaseInitializer`: الحالة (Unreachable/Missing/PendingMigrations/UpToDate/NewerThanApp)، الإنشاء بالترتيب العربي، تفعيل `READ_COMMITTED_SNAPSHOT`، **التطبيق التلقائي للترحيلات وفق D-043** (المهام 2.21–2.27)، مع تقارير تقدم (`IProgress<StartupProgress>`: اسم الخطوة والترحيل الجاري) تعرضها شاشة البداية (4.14).
- [x] 2.15 `IUnitOfWork.ExecuteInTransactionAsync` مع `CreateExecutionStrategy` و`EnableRetryOnFailure`.
- [x] 2.16 `ISequenceAllocator` (UPDATE…OUTPUT مع UPDLOCK)، `IServerClock` (`SYSDATETIME()`).
- [x] 2.17 `ISettingsService`: قراءة مكتوبة النوع بذاكرة مؤقتة، حفظ مع تدقيق، قفل `Currency.Decimals` بعد أول اعتماد.
- [x] 2.18 ترجمة أخطاء SQL الشائعة (تكرار مفتاح فريد 2601/2627، FK 547، تعارض تزامن) إلى `Error` عربي.
- [x] 2.19 اختبارات التكامل TS-010…TS-014 + اختبار البذور idempotent (تشغيل مرتين).
- [x] 2.20 تحديث سجل التغييرات في DATABASE_DESIGN.md بأي انحراف.

**الترحيل التلقائي الآمن (D-043 — طلب المالك):**
- [x] 2.21 قفل الترحيل على مستوى القاعدة: `sp_getapplock @Resource='Mizad.Migrations', @LockMode='Exclusive', @LockOwner='Session'` على اتصال مخصص مع مهلة، وإعادة فحص الترحيلات المعلقة بعد الحصول عليه؛ الأجهزة المنتظرة تُبلّغ «جارٍ تحديث قاعدة البيانات من جهاز آخر…». (EF Core 9+ يقفل `Migrate` داخلياً أيضاً — قفلنا يغطي النسخة + الترحيل + البذور كوحدة واحدة.)
- [x] 2.22 خدمة نسخ احتياطي أساسية `IDatabaseBackupService.BackupAsync(BackupType)`: `BACKUP DATABASE … WITH COPY_ONLY, CHECKSUM, INIT` إلى `Backup.Path` إن ضُبط وإلا `SERVERPROPERTY('InstanceDefaultBackupPath')`، ثم `RESTORE VERIFYONLY WITH CHECKSUM`، وتسجيل في `BackupHistory` و`backup-log.json`. تُستخدم هنا لنسخة `PreMigration` الإلزامية، وتوسعها Phase 16 (الجدولة، الاحتفاظ، الاستعادة، الواجهة).
- [x] 2.23 تسلسل الترحيل التلقائي: قفل ⇐ نسخة PreMigration (فشلها يوقف الترحيل) ⇐ `MigrateAsync` (كل ترحيل في معاملة) ⇐ مزامنة البذور ⇐ تحقق «لا معلق» ⇐ تحرير القفل ⇐ تدقيق. فشل ترحيل ⇐ نتيجة خطأ عربية برقم مرجعي، والقاعدة على آخر ترحيل سليم.
- [x] 2.24 مزامنة البذور (Seed Sync) idempotent بعد كل ترحيل: إضافة الصلاحيات الجديدة لدور المدير، مفاتيح الإعدادات الجديدة بقيمها الافتراضية، القيم المرجعية الجديدة — دون تعديل بيانات المستخدم القائمة.
- [x] 2.25 فحص توافق الإصدار: القاعدة أحدث من البرنامج (ترحيلات مطبقة غير معروفة للبرنامج) ⇐ حالة `NewerThanApp` تمنع التشغيل برسالة التحديث.
- [x] 2.26 حراسة الترحيلات (اختبارات): `HasPendingModelChanges() == false`؛ تطبيق كل الترحيلات على قاعدة فارغة؛ التطبيق مرتين بلا أثر؛ 3 مُهيئات متزامنة ⇐ ترحيل واحد ونسخة واحدة بلا أخطاء؛ حقن فشل في ترحيل ⇐ القاعدة سليمة على آخر ترحيل؛ فحص ملفات الترحيل الجديدة بحثاً عن `DropTable/DropColumn/RenameColumn` غير المدرجة في قائمة الاستثناء (TS-015…TS-020). — ✅ + اختبار فشل ترحيل فعلي (قاعدة للقراءة فقط بعد النسخة) وحالة خادم غير متاح.
- [x] 2.27 توثيق «سياسة الترحيلات» ودليل إضافة ترحيل جديد في DATABASE_DESIGN §14، ومنح حساب التطبيق `db_owner` على قاعدة مِيزاد فقط في DEPLOYMENT.

### Related UI
لا شيء (تقارير التقدم تُعرض في شاشة البداية — 4.14).

### Implementation
`Mizad.Domain/<Module>/*` الكيانات؛ `Mizad.Infrastructure/Persistence/{MizadDbContext, Configurations/, Migrations/, Seed/, Interceptors/, SqlHelpers/, DatabaseInitializer.cs, MigrationLock.cs}`؛ `Mizad.Infrastructure/Backup/DatabaseBackupService.cs`؛ `Mizad.Application/Abstractions/*`.

### Database Changes
إنشاء كامل المخطط (الترحيل `InitialCreate`).

### Testing
TS-010، TS-011، TS-012، TS-013، TS-014؛ TS-015…TS-020 (الترحيل التلقائي)؛ اختبار وحدات `ArabicNormalizer` (TS-807).

### Acceptance Criteria
- الترحيل يُنشئ قاعدة بكل الجداول والقيود والفهارس الموثقة والترتيب العربي وRCSI.
- البذور كاملة وidempotent، ومزامنتها تضيف الجديد فقط.
- تخصيص الأرقام متزامن بلا تكرار ولا فجوات.
- **الترحيل تلقائي بالكامل دون تدخل المستخدم، مسبوق دائماً بنسخة احتياطية متحقق منها، وآمن مع عدة أجهزة تُقلع معاً.**
- `HasPendingModelChanges` = false؛ كل اختبارات P2 تمر؛ لا تحذيرات.

### Risks
- تفعيل RCSI يتطلب عدم وجود اتصالات أخرى — يتم لحظة الإنشاء فقط.
- عدد الجداول كبير — راجع كل إعداد مقابل DATABASE_DESIGN بنداً بنداً.
- R-18 فشل ترحيل لدى عميل — النسخة المسبقة + المعاملات + اختبارات الترقية من لقطات الإصدارات.
- مسار النسخ الافتراضي لنسخة SQL قد يكون على قرص ممتلئ — رسالة واضحة وتوقف آمن قبل الترحيل.

### Completion Evidence
- **البناء:** `./scripts/build.ps1` ⇐ **0 Warning(s), 0 Error(s)**.
- **الاختبارات:** `dotnet test --solution Mizad.slnx -c Release` ⇒ **total 85، succeeded 85، failed 0** — وحدات 52 (منها: حراسة النموذج والترحيلات 7 [TS-015، TS-020]، التطبيع العربي 13 [TS-807]، الإعدادات والتسلسلات 5)؛ تكامل 33 على SQL Server 2025 (المخطط والبذور TS-010، القيود TS-011/012/014، التسلسلات المتوازية TS-013، الخدمات، المُهيئ TS-016…TS-020 + فشل ترحيل فعلي + خادم غير متاح).
- **مراجعة سكربت الترحيل** (`artifacts/sql/migrate.sql`، غير مُتتبع): 45 جدولاً + `__EFMigrationsHistory`، 66 قيد CHECK، 152 مفتاحاً خارجياً، الحذف المتتالي في 11 علاقة مسموحة فقط، 46 فهرساً فريداً (17 مرشّح)، 4 أعمدة max مقصودة فقط، لا float، لا `ALTER DATABASE`، 22 عمود rowversion.
- **النظافة:** لا قواعد `Mizad_IT_*` متبقية، ولا سجلات نسخ لها في msdb، ولا ملفات نسخ اختبار في مجلد النسخ (تحقق عبر `sys.dm_os_enumerate_filesystem`).
- **التشغيل:** `Mizad.exe` يُقلع ويستجيب ويُغلق برمز 0 بعد إضافة خدمات الطبقة.
- **ملفات جديدة:** `dotnet-tools.json`؛ `src/Mizad.Domain/{Common/DocumentEntity.cs, Common/SharedEnums.cs, Calculations/ArabicNormalizer.cs, Administration/*, Identity/*, Catalog/*, Inventory/*, Parties/*, Sales/*, Purchasing/*, Returns/*, Treasury/*}`؛ `src/Mizad.Application/Abstractions/{Persistence/*, Database/*}`؛ `src/Mizad.Infrastructure/{Persistence/*, Persistence/Configurations/*, Persistence/Seed/*, Persistence/Migrations/*, Backup/*}`؛ `tests/Mizad.IntegrationTests/{TestDatabase.cs, TestData.cs, Persistence/*}`؛ `tests/Mizad.UnitTests/{Domain/ArabicNormalizerTests.cs, Domain/SettingsAndSequencesTests.cs, Infrastructure/ModelAndMigrationGuardTests.cs}`.
- **ملفات معدلة:** `Directory.Packages.props` (EF Core 10.0.14، DI/Logging)، csproj لـ Application/Infrastructure/IntegrationTests، `.editorconfig` (مجلد الترحيلات كود مولّد)، `Common/Entity.cs` (`IHasCreationStamp`، `LedgerRecord`)، `IAuditLogger` (التوقيع `Log(context, entry)`)، `DependencyInjection.cs`، `SqlServerFixture.cs`.

### Completion Summary
قاعدة البيانات كاملة وفق DATABASE_DESIGN (45 جدولاً) بترحيل `InitialCreate`، مع **ترحيل تلقائي آمن** (D-043): قفل على مستوى القاعدة، نسخة PreMigration متحقق منها قبل أي ترحيل (وفشلها يمنع الترحيل)، ترحيلات واحداً واحداً بتقارير تقدم عربية، مزامنة بذور، تدقيق، ورفض القاعدة الأحدث من البرنامج. خدمات البنية جاهزة: وحدة العمل بالمعاملات وإعادة المحاولة، الترقيم بلا فجوات، ساعة الخادم، سجل التدقيق، الإعدادات المكتوبة بالنوع مع قفل منازل العملة، ترجمة أخطاء SQL للعربية، وتصنيف أخطاء الاتصال (أساس D-046).
**انحرافات موثقة** في DATABASE_DESIGN §13: الترتيب في `CREATE DATABASE` لا في النموذج؛ بذّار صريح بدل `UseAsyncSeeding`؛ `RolePermissionGrant`؛ حالات الجرد عبر `DocumentStatus` + `IsSubmitted`؛ `ReturnSettlementMethod.PartyCredit` مشترك؛ فهرس البنود المفتوحة بديل (الفهارس المرشحة لا تقارن عمودين).
**ملاحظات للمراحل التالية:** أي تعديل على الكيانات = `dotnet ef migrations add <Name> --project src/Mizad.Infrastructure --startup-project src/Mizad.Infrastructure --output-dir Persistence/Migrations` ثم اختبار `HasPendingModelChanges`. أي صلاحية/إعداد/قيمة مرجعية جديدة تضاف إلى `DatabaseSeeder` أو `SettingKeys`. البحث العربي يقارن `SearchName` بـ `ArabicNormalizer.Normalize(text)`.

---

## Phase 3 — Authentication & Security Foundation

Status: Not Started

### Objective
تسجيل الدخول الآمن، الجلسة، الصلاحيات المركزية، سجل التدقيق، والتهيئة الأولى للمدير — منطقاً خلفياً مختبراً (الواجهات في P4).

### Dependencies
P2.

### Tasks
- [ ] 3.1 كتالوج الصلاحيات `Permissions` (الرموز، الأسماء العربية، الوحدة، قياسي/خاص) وفق BUSINESS_RULES §9 + اختبار تفرد واكتمال الأسماء.
- [ ] 3.2 بذر الأدوار الأربعة وصلاحياتها (BUSINESS_RULES §9.3).
- [ ] 3.3 `Pbkdf2PasswordHasher` (D-017) + `NeedsRehash`.
- [ ] 3.4 مدقق سياسة كلمة المرور (≥ 8 أحرف، لا تساوي اسم المستخدم).
- [ ] 3.5 `AuthService.LoginAsync/LogoutAsync`: تطبيع الاسم، القفل بعد المحاولات، رفض المعطل، `LastLoginAtUtc`، `MustChangePassword`، تدقيق النجاح والفشل.
- [ ] 3.6 `CurrentUserSession` (تنفيذ `ICurrentUser`) + `AuthorizationService.Demand/Has` + خطأ `Forbidden`.
- [ ] 3.7 `AuditLogger` يكتب ضمن معاملة العملية الجارية.
- [ ] 3.8 `SetupService`: `GetSetupStateAsync`، `CreateInitialAdminAsync` (يفشل إن وُجد أي مستخدم — فحص داخل معاملة Serializable).
- [ ] 3.9 `UserService.ChangePasswordAsync`، `ResetPasswordAsync` (يضبط `MustChangePassword`)، منع تعطيل آخر مدير نشط.
- [ ] 3.10 اختبارات الوحدات (المجزئ، السياسة، منطق القفل بساعة وهمية) والتكامل TS-050…TS-055.

### Related UI
UI-01، UI-11 (لاحقاً).

### Implementation
`Application/Identity/*`، `Infrastructure/Security/*`، `Infrastructure/Persistence/Seed/RolesSeeder.cs`.

### Database Changes
لا تغيير في المخطط؛ بذور الأدوار والصلاحيات.

### Testing
TS-050…TS-055؛ وحدات المجزئ (تحقق، كلمة خاطئة، صيغة، ملح مختلف لكل تجزئة).

### Acceptance Criteria
- لا كلمة مرور نصية في أي مكان (DB، سجل، تدقيق).
- كل خدمة كتابة تستدعي `Demand` (اختبار انعكاسي TS-053 يبدأ هنا ويتوسع مع كل مرحلة).
- المدير الأول يُنشأ مرة واحدة فقط.

### Risks
- 600k تكرار قد يبطئ الدخول على أجهزة ضعيفة — قِس الزمن (هدف ≤ 1 ث) ووثّقه؛ خفّض بقرار جديد إن لزم.

### Completion Evidence
—

### Completion Summary
—

---

## Phase 4 — Design System & Application Shell

Status: Not Started

### Objective
نظام التصميم الكامل (الخط، الألوان، الأيقونات، الأنماط، المكونات)، النافذة المخصصة، الشريط الجانبي والترويسة والتنقل، وتسلسل الإقلاع بشاشات الاتصال والتهيئة والدخول.

### Dependencies
P3.

### Tasks
- [ ] 4.1 تضمين خطوط Tajawal (Regular, Medium, Bold, ExtraBold) كـ Resource في `Assets/Fonts` + `OFL.txt`؛ `FontFamily` عام على مستوى التطبيق.
- [ ] 4.2 `Themes/Colors.xaml` و`Typography.xaml` و`Metrics.xaml` من UI_IMPLEMENTATION_MAP §2 (لا ألوان مضمنة في Views).
- [ ] 4.3 `Themes/Icons.xaml`: أيقونات Lucide المطلوبة (~70) كـ Geometry + `THIRD_PARTY_NOTICES.md`.
- [ ] 4.4 أصول الشعار من `Logo.png`: نسخة بنص أبيض للخلفيات الداكنة، أيقونة فقط، `app.ico` (16–256)، رسم شاشة الدخول. إن تعذّرت الجودة اطلب من المالك ملفاً متجهياً (R-14).
- [ ] 4.5 `Localization/MizadCulture`: ثقافة عربية بتقويم ميلادي وأسماء أشهر يناير…ديسمبر وأرقام لاتينية؛ ضبط `FrameworkElement.LanguageProperty`؛ اختبارات تنسيق (`28 يناير 2025`، `12,600`).
- [ ] 4.6 أنماط الأزرار (Primary/Navy/Outline/Danger/Ghost/Icon/Link)، الحقول (بأيقونة، Placeholder، حالة خطأ)، PasswordBox بزر إظهار، ComboBox، DatePicker (عربي ميلادي)، CheckBox، RadioButton، ToggleSwitch.
- [ ] 4.7 نمط DataGrid موحد (رأس، صفوف، تحديد، أعمدة مبالغ LTR، Virtualization مفعلة افتراضياً) + `Pagination`.
- [ ] 4.8 المكونات المخصصة: `StatCard`، `StatusBadge`، `SearchBox` (Debounce)، `QuantityStepper`، `SegmentedTabs`، `SummaryRow`، `PaymentTypeCard`، `EmptyState`، `LoadingOverlay`، `Card`، `PageHeader`، `Breadcrumb`.
- [ ] 4.9 `MizadWindow` بـ `WindowChrome`: العنوان والشعار يميناً، أزرار التحكم يساراً، Snap، حد أدنى 1280×720.
- [ ] 4.10 `INavigationService` (ViewModel-first، سجل مسارات بالأيقونة والصلاحية، حارس التغييرات غير المحفوظة)، `ViewMappings.xaml`.
- [ ] 4.11 الشريط الجانبي (D-024، عناصر حسب الصلاحية، قابل للطي < 1400px والتمرير) والترويسة (التاريخ، اسم المحل، بحث عام [placeholder حتى 14.6]، جرس [placeholder حتى 14.7]، قائمة المستخدم).
- [ ] 4.12 `IDialogService` (حوارات مشروطة، تأكيد، تأكيد بكتابة كلمة للعمليات المدمرة) و`IToastService`.
- [ ] 4.13 Spike LiveCharts2: منحنى + دائري بخط Tajawal عبر SkiaSharp (تسجيل الخط وتشكيل العربية HarfBuzz)، في RTL؛ النتيجة في DECISIONS (تأكيد D-032 أو بديل).
- [ ] 4.14 **شاشة البداية `SplashWindow` (S33) + `StartupOrchestrator`** (D-044، ARCHITECTURE §4.9): نافذة بالشعار والاسم والإصدار، نص الخطوة الجارية وشريط تقدم، والمنسق يعمل في الخلفية: تحميل الإعدادات ⇐ ملف اتصال؟ (لا ⇐ معالج الإعداد 4.15) ⇐ اختبار الاتصال (مهلة 15 ث + 3 محاولات) ⇐ `DatabaseInitializer` (إنشاء/ترحيل تلقائي بتقارير تقدم — 2.14، 2.23) ⇐ مستخدمون؟ (لا ⇐ 4.16) ⇐ الإعدادات المشتركة ⇐ الدخول. عند الفشل لوحة سبب عربية مع: «إعادة المحاولة»، «إعدادات الاتصال»، «نسخ تفاصيل الدعم»، «خروج». لا إغلاق صامت.
- [ ] 4.15 **معالج الإعداد للتنصيب الجديد** `ConnectionSetupView` (S24): اختيار النمط «هذا الجهاز هو الخادم» (اكتشاف نسخ SQL Server المحلية من الخدمات، إنشاء قاعدة جديدة) أو «الاتصال بخادم في الشبكة» (اسم الخادم/النسخة، Windows/SQL)؛ اختبار الاتصال برسائل أسباب واضحة؛ اختيار قاعدة قائمة مع التحقق أنها قاعدة مِيزاد (جدول `__EFMigrationsHistory` + ترحيلات مِيزاد) أو إنشاء جديدة؛ حفظ `connection.json`؛ العودة لشاشة البداية لتكمل تلقائياً.
- [ ] 4.16 `SetupWizardView` (S25): المحل ⇐ العملة ومنازلها ⇐ المدير وكلمة مروره (يُفتح تلقائياً عند قاعدة بلا مستخدمين).
- [ ] 4.17 `LoginView` (S01، UI-01) مع DV-06 (Caps Lock، تذكر الاسم فقط، الإصدار).
- [ ] 4.18 `ChangePasswordDialog` (S26) + الإجبار عند `MustChangePassword` + تسجيل الخروج.
- [ ] 4.19 `DesignGalleryView` (Debug فقط) يعرض كل المكونات وحالاتها للمراجعة البصرية.
- [ ] 4.20 اختبارات ViewModels: الدخول، التنقل وحارسه، الشريط حسب الصلاحيات، الثقافة.
- [ ] 4.21 مراجعة بصرية مقابل UI-01 وإطار UI-02 بلقطات في `docs/screenshots/phase-04/` وفق قائمة §7 من UI_IMPLEMENTATION_MAP.

**العمليات في الخلفية والتحميل وحالة الاتصال (D-045، D-046 — طلب المالك):**
- [ ] 4.22 `ViewModelBase.RunAsync(operation, BusyScope)` + `IBusyTracker`: تنفيذ في الخلفية، حالة انشغال للزر/المنطقة/الصفحة، إلغاء ومهلة، تحويل الأخطاء لرسائل عربية، إبلاغ مراقب الاتصال؛ أنماط تحميل موحدة: زر بمؤشر دوّار، `LoadingOverlay` للمنطقة، شريط تقدم مع إلغاء للعمليات الطويلة، **شريط تقدم رفيع عام في الترويسة**.
- [ ] 4.23 `IConnectionMonitor` (Application) وتنفيذه (Infrastructure): الحالات Connected/Reconnecting/Disconnected، مصنّف أخطاء SqlException العابرة، فحص `SELECT 1` كل 30 ث (مهلة 5 ث)، إعادة اتصال بتراجع أُسّي حتى 30 ث، حدث تغيّر الحالة، تسجيل في السجل.
- [ ] 4.24 واجهة حالة الاتصال: مؤشر دائم في الترويسة (أخضر «متصل» / كهرماني «جارٍ إعادة الاتصال…» / أحمر «غير متصل»)، شريط تنبيه أعلى المحتوى عند الانقطاع مع «إعادة المحاولة الآن»، إشعار عند العودة؛ ربط `CanExecute` لأوامر الكتابة بحالة الاتصال؛ وسم البيانات المعروضة «قد لا تكون محدّثة» أثناء الانقطاع.
- [ ] 4.25 `IRecoveryStore`: حفظ محلي تلقائي لمسودات الإدخال غير المحفوظة في `%LocalAppData%\Mizad\recovery\` واستعادتها بعد انقطاع أو إغلاق مفاجئ (تستخدمه نقطة البيع 9.12 وأي نموذج طويل).
- [ ] 4.26 اختبارات: `StartupOrchestrator` بخدمات وهمية (كل فرع: لا اتصال، قاعدة مفقودة، ترحيل، أحدث من البرنامج، بلا مستخدمين، نجاح)، `ConnectionMonitor` بمِسبار وهمي وساعة وهمية (الانتقالات والتراجع)، `RunAsync` (انشغال، إلغاء، خطأ اتصال ⇐ إبلاغ المراقب)، تكامل: قطع الجلسة بـ `KILL` ⇐ الحالة Disconnected ثم Connected (TS-060…TS-064).

### Related UI
UI-01، UI-02 (الإطار + مؤشر الاتصال في الترويسة)، UI-11 (الترويسة)، LOGO؛ شاشات مشتقة: S33 شاشة البداية، S24 معالج الاتصال، S25 التهيئة.

### Implementation
`Mizad.Desktop/{Assets, Themes, Controls, Shell, Localization, Startup (SplashWindow, StartupOrchestrator), Features/Identity, Features/Setup}`؛ `Mizad.Application/Abstractions/{IConnectionMonitor, IBusyTracker, IRecoveryStore}`؛ `Mizad.Infrastructure/Connectivity/*`.

### Database Changes
لا شيء.

### Testing
اختبارات ViewModels المذكورة؛ اختبار الثقافة؛ TS-060…TS-064؛ مراجعة بصرية يدوية موثقة.

### Acceptance Criteria
- تثبيت نظيف بلا اتصال ⇐ شاشة البداية ⇐ معالج الإعداد ⇐ إنشاء القاعدة ⇐ التهيئة ⇐ الدخول ⇐ الإطار يعمل من البداية للنهاية **دون أي خطوة يدوية على قاعدة البيانات**.
- تشغيل إصدار أحدث على قاعدة قديمة يطبق الترحيلات تلقائياً في شاشة البداية بعد نسخة احتياطية، ويعرض التقدم.
- لا تتجمد الواجهة في أي عملية؛ لكل عملية مؤشر تحميل.
- إيقاف خدمة SQL أثناء التشغيل يُظهر «غير متصل» خلال ≤ 30 ث، ويعطّل أوامر الكتابة، ويعود تلقائياً عند عودة الخدمة دون إعادة تشغيل البرنامج.
- كل النصوص بخط Tajawal المُضمن، RTL صحيح، لا ألوان مضمنة خارج Themes.
- الشريط يعرض العناصر حسب الصلاحيات؛ التنقل يعمل بلوحة المفاتيح.
- لقطات المراجعة البصرية محفوظة والفروقات موثقة.

### Risks
- R-02 الرسوم وتشكيل العربية في SkiaSharp.
- R-14 جودة الشعار.
- `WindowChrome` مع RTL قد يعكس أزرار التحكم بشكل غير متوقع — اختبر مبكراً.
- R-19 شبكات محلية غير مستقرة — مراقب الاتصال + عدم فقدان الإدخال + التحقق بـ `ClientRequestId`.

### Completion Evidence
—

### Completion Summary
—

---

## Phase 5 — Products & Categories Management

Status: Not Started

### Objective
إدارة الأصناف والتصنيفات والوحدات والباركود والأسعار والصور والبحث والاستيراد (UI-06).

### Dependencies
P4.

### Tasks
- [ ] 5.1 `CategoryService` و`UnitService` (CRUD، تفرد الاسم، تعطيل بدل حذف عند الاستخدام) + `CategoriesDialog`/`UnitsDialog` (S10).
- [ ] 5.2 `ProductService`: إنشاء/تعديل/تعطيل/حذف (الحذف فقط بلا مراجع)، الكود التلقائي، التحقق (FluentValidation).
- [ ] 5.3 الوحدات المتعددة: جدول الوحدات بالمعامل والأسعار والافتراضيات؛ وحدة أساسية واحدة بمعامل 1؛ تعطيل الوحدة المستخدمة.
- [ ] 5.4 الباركود: متعدد لكل وحدة، فريد نظامياً، توليد EAN-13 داخلي (200–299)، تحقق خانة الفحص للأكواد الرقمية القياسية.
- [ ] 5.5 `ProductLookupService`: باركود مطابق (≤ 100 مللي ث)، بحث بالاسم المطبَّع والكود، ترقيم ومرشحات وفرز على الخادم.
- [ ] 5.6 الصور: اختيار، تصغير ≤ 512px و≤ 300KB، مصغرة 64px، حفظ في `ProductImages` (D-031).
- [ ] 5.7 `ProductsView` (UI-06): الشريط، الجدول، الترقيم، أعمدة التكلفة حسب `Inventory.ViewCost`.
- [ ] 5.8 `ProductEditorPanel` (UI-06) مع DV-10: حقل الرصيد الافتتاحي ظاهر ومعطل بتلميح حتى 7.8.
- [ ] 5.9 الاستيراد من Excel (S11): قالب للتنزيل، قراءة ClosedXML، معاينة بأخطاء لكل صف، إنشاء التصنيفات/الوحدات المفقودة اختيارياً، تنفيذ كله-أو-لاشيء، تقرير (عمود الرصيد الافتتاحي يُفعّل في 7.8).
- [ ] 5.10 اختبارات: TS-807، TS-808، TS-809 + تكامل (CRUD، تفرد الباركود، البحث العربي المطبَّع، التعطيل، الاستيراد الذري) + ViewModels.
- [ ] 5.11 مراجعة بصرية UI-06.

### Related UI
UI-06.

### Implementation
`Domain/Catalog`، `Domain/Calculations/{Ean13, ArabicNormalizer}`، `Application/Catalog/*`، `Infrastructure/Documents/ExcelImport`، `Desktop/Features/Catalog/*`.

### Database Changes
لا تغيير متوقع (إن لزم ترحيل يوثق).

### Testing
انظر 5.10.

### Acceptance Criteria
- إدارة كاملة للأصناف بوحدات وباركودات متعددة، بحث عربي مطبَّع سريع، استيراد ذري مع تقرير.
- لا يمكن تكرار الباركود؛ الصنف المستخدم لا يُحذف.
- مراجعة UI-06 موثقة.

### Risks
- ملفات Excel غير نظيفة — معاينة وتقرير أخطاء واضح.

### Completion Evidence
—

### Completion Summary
—

---

## Phase 6 — Customers & Suppliers Management

Status: Not Started

### Objective
إدارة العملاء والموردين، الأرصدة الافتتاحية، ودفتر الأطراف ومحرك المطابقة الذي تعتمد عليه كل المستندات اللاحقة.

### Dependencies
P4. (يمكن التوازي مع P5.)

### Tasks
- [ ] 6.1 `CustomerService` (الشريحة، الآجل، حد الائتمان [`Customers.EditCredit`]، أيام السداد، الحالة؛ حماية الزبون النقدي؛ تحذير تكرار الهاتف).
- [ ] 6.2 `SupplierService` (الشخص المسؤول، البريد بتحقق، الوصف، أيام السداد).
- [ ] 6.3 `IPartyLedger`: إضافة قيد، قيد عكسي، استعلام الرصيد (BR-051).
- [ ] 6.4 `FifoSettlementCalculator` (Domain نقي) + `ISettlementEngine`: تسوية صريحة، FIFO للطرف، فك تسويات قيد، إعادة احتساب `SettledAmount`، فحص الثابت (D-007، BR-052).
- [ ] 6.5 `OpeningBalanceService` (مستند OB-، صلاحية `Parties.OpeningBalance`، إلغاء بالعكس).
- [ ] 6.6 `PartyQueryService`: القوائم بالرصيد وآخر عملية والحالة (نشط/متأخر/موقوف)، مؤشرات الملف.
- [ ] 6.7 `CustomersView` (UI-08): القائمة، الملف، المؤشرات، التبويبات (الكشف/الفواتير/الدفعات تُربط في P9/P11 — تظهر حالة فارغة)، `CustomerEditDialog`.
- [ ] 6.8 `SuppliersView` (UI-09) + `SupplierEditDialog`.
- [ ] 6.9 `OpeningBalanceDialog` (S31).
- [ ] 6.10 اختبارات: وحدات FIFO (حالات: دائن أكبر، مدينات متعددة، صريح ثم FIFO، فك وإعادة)، تكامل (الإشارات، الرصيد الافتتاحي، TS-407 مبدئياً، حماية الزبون النقدي)، ViewModels.
- [ ] 6.11 مراجعة بصرية UI-08 وUI-09.

### Related UI
UI-08، UI-09.

### Implementation
`Domain/Parties`، `Domain/Calculations/FifoSettlementCalculator`، `Application/Parties/*`، `Desktop/Features/Parties/*`.

### Database Changes
لا تغيير متوقع.

### Testing
انظر 6.10.

### Acceptance Criteria
- الأطراف تُدار بالكامل؛ الأرصدة الافتتاحية موثقة كمستندات وتظهر في الرصيد.
- محرك المطابقة مختبر ويحافظ على الثابت.

### Risks
- تعقيد المطابقة — اختبارات وحدات شاملة قبل استخدامه في P8.

### Completion Evidence
—

### Completion Summary
—

---

## Phase 7 — Inventory Core & Stock Movement Engine

Status: Not Started

### Objective
محرك المخزون الذري بالمتوسط المرجح، الأرصدة والحركات وكارت الصنف والتنبيهات، وشاشة المخزون (UI-07) للعرض.

### Dependencies
P5.

### Tasks
- [ ] 7.1 `AverageCostCalculator` (Domain نقي — D-005، BR-010) + TS-001.
- [ ] 7.2 `IInventoryEngine.ApplyAsync(StockPosting)`: يعمل داخل معاملة المستدعي فقط؛ ترتيب الأصناف تصاعدياً؛ قفل `Products` بـ UPDLOCK؛ الصادر بتحديث ذري مشروط على `StockBalances`؛ إنشاء الرصيد عند أول وارد؛ تحديث `AverageCost`؛ تسجيل الحركة بـ `BalanceAfter`/`TotalOnHandAfter`/`AverageCostAfter`.
- [ ] 7.3 خطأ `InsufficientStock` بالاسم والمتاح؛ احترام `Inventory.AllowNegativeStock`.
- [ ] 7.4 `StockQueryService`: الأرصدة لكل مخزن، القيمة، كارت الصنف برصيد تراكمي، تحت الحد، النافد، الراكد (BR-018)، آخر حركة.
- [ ] 7.5 `WarehouseService` (CRUD، مخزن افتراضي واحد، تعطيل بدل حذف) + إعداد المخزن الافتراضي للجهاز (`machine.json`).
- [ ] 7.6 `DiagnosticsService.CheckStockReconciliation()` (BR-017).
- [ ] 7.7 `InventoryView` (UI-07): المؤشرات، المرشحات، الجدول، حركات الصنف؛ لوحة التسوية معطلة حتى 13.1.
- [ ] 7.8 الأرصدة الافتتاحية: تفعيل الحقل في محرر الصنف (مخزن + كمية + تكلفة ⇒ تسوية بسبب «رصيد افتتاحي»)؛ مستند أرصدة افتتاحية جماعي؛ عمود الرصيد في استيراد Excel.
- [ ] 7.9 اختبار معماري TS-604 (لا كتابة في StockBalances خارج المحرك).
- [ ] 7.10 اختبارات التكامل TS-002، TS-003، TS-004، TS-005.
- [ ] 7.11 مراجعة بصرية UI-07 (الأجزاء المنفذة).

### Related UI
UI-07، UI-06 (الرصيد الافتتاحي).

### Implementation
`Domain/Inventory`، `Domain/Calculations/AverageCostCalculator`، `Application/Inventory/*`، `Infrastructure/Inventory/InventoryEngine.cs`، `Desktop/Features/Inventory/*`.

### Database Changes
لا تغيير متوقع (فهارس إضافية إن أظهر الأداء حاجة — توثق).

### Testing
TS-001، TS-002…TS-005، TS-604.

### Acceptance Criteria
- 20 عملية صرف متوازية لا تنتج رصيداً سالباً ولا حركات زائدة.
- المطابقة تمر بعد كل سيناريو؛ المتوسط مطابق للحسابات اليدوية.
- لا توجد طريقة لتعديل الرصيد دون حركة.

### Risks
- R-03 الـ Deadlock — ترتيب الأقفال واختبار TS-003.

### Completion Evidence
—

### Completion Summary
—

---

## Phase 8 — Purchases Management

Status: Not Started

### Objective
نواة الخزينة (الحسابات والحركات والسندات التلقائية) وفواتير الشراء كاملة بمسوداتها واعتمادها وإلغائها وقائمتها (UI-05).

### Dependencies
P6، P7.

### Tasks
- [ ] 8.1 `ICashLedger`: ترحيل حركة، فحص السالب بـ UPDLOCK على `CashAccounts` (BR-061)، الرصيد، حركة عكسية.
- [ ] 8.2 `VoucherPostingService` (داخلي): إنشاء سند تلقائي (RC/PV) لفاتورة/مرتجع ⇐ حركة نقدية ⇐ قيد طرف ⇐ تسوية صريحة.
- [ ] 8.3 `InvoiceCalculator` (Domain — BR-091) مشترك للبيع والشراء + TS-801…TS-805.
- [ ] 8.4 `PurchaseInvoiceService`: مسودة (إنشاء/تعديل/حذف)، `PostAsync` (BR-030، BR-031، BR-032، D-011)، `VoidAsync` (BR-034).
- [ ] 8.5 `PurchaseQueryService`: القائمة بالمرشحات والمؤشرات، التفاصيل.
- [ ] 8.6 `PurchaseInvoiceView` (UI-05): المسار، الرأس، البحث، الجدول، الدفع، الملخص، الأزرار؛ اختصارات لوحة المفاتيح.
- [ ] 8.7 «إضافة صنف جديد» من الفاتورة (إعادة استخدام محرر الصنف كحوار).
- [ ] 8.8 `PurchaseInvoicesView` (S08) بنمط UI-04.
- [ ] 8.9 اقتراح تحديث سعر البيع بعد الاعتماد (BR-033).
- [ ] 8.10 اختبارات TS-101…TS-105 + ViewModels + idempotency.
- [ ] 8.11 مراجعة بصرية UI-05.

### Related UI
UI-05، UI-04 (نمط القائمة).

### Implementation
`Domain/{Purchasing, Treasury}`، `Application/{Purchasing, Treasury/Core}`، `Desktop/Features/Purchasing/*`.

### Database Changes
لا تغيير متوقع.

### Testing
TS-101…TS-105، TS-801…TS-805.

### Acceptance Criteria
- الاعتماد ذري ويحدّث المخزون والمتوسط ودائن المورد والنقد معاً؛ الإلغاء يعكس بدقة أو يُرفض بسبب واضح.
- لا تكرار عند الضغط المزدوج.

### Risks
- توزيع خصم الفاتورة على التكلفة — اختبارات دقيقة.

### Completion Evidence
—

### Completion Summary
—

---

## Phase 9 — Sales & POS Management

Status: Not Started

### Objective
نقطة البيع السريعة (UI-03) وإدارة فواتير المبيعات (UI-04) بكل قواعد الخصم والسعر والائتمان والتكلفة والتزامن.

### Dependencies
P6، P7، P8 (8.1–8.3).

### Tasks
- [ ] 9.1 `SalesInvoiceService`: مسودة/تعليق، `PostAsync` (BR-020…BR-026)، `VoidAsync` (BR-028)، idempotency.
- [ ] 9.2 الضريبة الاختيارية في الحاسبة والعرض (D-014).
- [ ] 9.3 `SalesQueryService`: القائمة بالمؤشرات ومقارنة الفترة السابقة والمرشحات والفرز والترقيم؛ التفاصيل مع الدفعات والمرتجعات؛ الفواتير المعلقة.
- [ ] 9.4 `SalesPosViewModel`: إدخال الباركود (Keyboard Wedge، Enter)، قائمة نتائج البحث بالأسهم، إضافة/زيادة السطر، الوحدة تعيد التسعير، Stepper، خصم السطر والفاتورة، العميل (F4، إضافة سريعة)، أنواع السداد ووسائله، المبلغ المستلم والباقي (DV-07)، الإتمام والتفريغ.
- [ ] 9.5 `SalesPosView` (UI-03) + اختصارات (UI_IMPLEMENTATION_MAP S04) + تركيز تلقائي دائم على حقل الباركود.
- [ ] 9.6 لوحة «الفواتير المعلقة» (F6) لاستئناف/حذف المسودات.
- [ ] 9.7 `SalesInvoicesView` (UI-04) مع DV-04، الإجراءات (عرض، طباعة [15.7]، مرتجع [10.6]، إلغاء، نسخ).
- [ ] 9.8 `SalesInvoiceDetailsView` (S06).
- [ ] 9.9 ربط تبويب «الفواتير» في ملف العميل (UI-08).
- [ ] 9.10 اختبارات TS-201…TS-215 + ViewModels (POS) + قياس مبدئي NFR-03/NFR-04.
- [ ] 9.11 مراجعة بصرية UI-03 وUI-04.
- [ ] 9.12 نقطة البيع عند انقطاع الاتصال (D-046): السلة تبقى في الذاكرة وتُحفظ تلقائياً عبر `IRecoveryStore` بعد كل تعديل وتُستعاد بعد إغلاق مفاجئ؛ زر الإتمام معطل أثناء الانقطاع برسالة واضحة؛ اعتماد انقطع أثناء التنفيذ ⇐ بعد العودة يُستعلم بـ `ClientRequestId` ويُعرض «تمت العملية» أو تُتاح إعادة المحاولة بأمان؛ مؤشر تحميل أثناء البحث والإضافة والاعتماد دون تجميد إدخال الماسح؛ اختبارات (TS-216، TS-217).

### Related UI
UI-03، UI-04، UI-08 (تبويب الفواتير).

### Implementation
`Domain/Sales`، `Application/Sales/*`، `Desktop/Features/Sales/*`.

### Database Changes
لا تغيير متوقع.

### Testing
TS-201…TS-215.

### Acceptance Criteria
- بيعة من 5 أصناف بالماسح دون فأرة ممكنة في < 30 ث.
- كل القواعد الحرجة 1، 2، 7، 8، 9، 10، 11 مثبتة باختبارات ناجحة.
- لا فاتورة معتمدة بلا حركاتها.

### Risks
- سرعة إدخال الماسح تتجاوز معالجة الواجهة — طابور إدخال وعدم حجب UI Thread.

### Completion Evidence
—

### Completion Summary
—

---

## Phase 10 — Sales & Purchases Returns

Status: Not Started

### Objective
مرتجعات البيع والشراء المرتبطة بالفواتير الأصلية مع حدود الكمية والتقييم والتسوية (UI-13…UI-17).

### Dependencies
P8، P9.

### Tasks
- [ ] 10.1 `ReturnCalculator` (Domain): صافي سعر الوحدة، قاعدة الإرجاع الكامل، المتاح (BR-041، BR-042) + TS-806.
- [ ] 10.2 `SalesReturnService`: مسودة، `PostAsync` (BR-040…BR-044، BR-046)، `VoidAsync` (BR-047).
- [ ] 10.3 `PurchaseReturnService`: مسودة، `PostAsync` (BR-045)، `VoidAsync`.
- [ ] 10.4 استعلام البحث عن الفاتورة الأصلية (رقم، طرف، فترة) وبيانات الفاتورة مع المرتجع السابق.
- [ ] 10.5 `ReturnsView` المركّبة (S20، DV-12، DV-13): التبويبان، البيانات، الفاتورة الأصلية، الأسطر، التفاصيل، الملخص، السجل والمؤشرات.
- [ ] 10.6 ربط إجراء «مرتجع» من قوائم فواتير البيع والشراء بفتح الشاشة محملة.
- [ ] 10.7 مؤشر «مرتجع جزئي/كلي» في قوائم الفواتير وتفاصيلها.
- [ ] 10.8 اختبارات TS-301…TS-309 + ViewModels.
- [ ] 10.9 مراجعة بصرية مقابل UI-14 (مع UI-15/16).

### Related UI
UI-13، UI-14، UI-15، UI-16، UI-17.

### Implementation
`Domain/Returns`، `Application/Returns/*`، `Desktop/Features/Returns/*`.

### Database Changes
لا تغيير متوقع.

### Testing
TS-301…TS-309، TS-806.

### Acceptance Criteria
- القواعد الحرجة 4، 5، 17 مثبتة؛ لا إرجاع متجاوز حتى بالتزامن؛ القيم تطابق الفاتورة الأصلية بلا فروق تقريب تراكمية.

### Risks
- التسوية مع فواتير مدفوعة جزئياً — سيناريوهات اختبار صريحة.

### Completion Evidence
—

### Completion Summary
—

---

## Phase 11 — Receivables & Payables

Status: Not Started

### Objective
التحصيل والتسديد والرد، كشوف الحساب، أعمار الديون، وربط شاشات العملاء والموردين بالكامل.

### Dependencies
P8، P9.

### Tasks
- [ ] 11.1 `VoucherService` لسندات الأطراف: تحصيل من عميل، تسديد لمورد، رد لعميل، استرداد من مورد؛ مسودة/اعتماد/إلغاء؛ تخصيص صريح لفواتير مختارة أو FIFO.
- [ ] 11.2 `StatementService`: كشف لفترة (BR-054) مع خيار إخفاء الملغى وعكسه.
- [ ] 11.3 أعمار الديون (BR-055) وأيام التأخر.
- [ ] 11.4 `PartyVoucherDialog` (S17): المبلغ، الطريقة، الحساب، التاريخ، اختيار الفواتير المفتوحة مع التوزيع.
- [ ] 11.5 ربط UI-08/UI-09: «تسجيل تحصيل/دفعة»، تبويبات الكشف والدفعات، المؤشرات الحقيقية، شارة «متأخر N أيام».
- [ ] 11.6 اختبارات TS-401…TS-407.
- [ ] 11.7 مراجعة بصرية UI-08/UI-09 كاملة.

### Related UI
UI-08، UI-09، UI-10 (نموذج السند).

### Implementation
`Application/Parties/{VoucherService, StatementService, AgingService}`، `Desktop/Features/Parties/*`.

### Database Changes
لا تغيير متوقع.

### Testing
TS-401…TS-407.

### Acceptance Criteria
- القاعدة الحرجة 6 مثبتة؛ الكشف يطابق الدفتر؛ ثابت المطابقة صحيح بعد كل سيناريو.

### Risks
- R-08 أداء FIFO لعميل كثير القيود — الفهرس المُفلتر للبنود المفتوحة.

### Completion Evidence
—

### Completion Summary
—

---

## Phase 12 — Cashbox & Expenses

Status: Not Started

### Objective
شاشة الصندوق (UI-10): الحسابات، السندات العامة، المصروفات، التحويلات، السجل، والورديات الاختيارية.

### Dependencies
P11.

### Tasks
- [ ] 12.1 إدارة `CashAccounts` (نقد/بنك، افتراضي لكل نوع، تعطيل) + الصندوق الافتراضي للجهاز.
- [ ] 12.2 السندات العامة (جهة «أخرى») بأسباب `VoucherReasons` (BR-062، BR-064).
- [ ] 12.3 `ExpenseService` + `ExpenseCategories` CRUD (BR-063).
- [ ] 12.4 `CashTransferService` (BR-065، `Cash.Transfer`).
- [ ] 12.5 `CashQueryService`: السجل برصيد بعد كل حركة، المؤشرات مع المقارنة، أعلى فئات المصروفات.
- [ ] 12.6 `CashboxView` (UI-10) + `VoucherPanel` + `ExpenseDialog` (S32) مع DV-18.
- [ ] 12.7 الورديات (D-029، BR-066): فتح/إغلاق، الوسم، الفرق، تقرير، اشتراط الوردية في نقطة البيع عند التفعيل، `ShiftDialog` (S19).
- [ ] 12.8 اختبارات TS-501…TS-504.
- [ ] 12.9 مراجعة بصرية UI-10.

### Related UI
UI-10.

### Implementation
`Application/Treasury/*`، `Desktop/Features/Treasury/*`.

### Database Changes
لا تغيير متوقع.

### Testing
TS-501…TS-504.

### Acceptance Criteria
- الرصيد = Σ الحركات دائماً؛ لا سالب عند المنع؛ المصروفات مصنفة وتظهر في صافي الربح لاحقاً.

### Risks
- الخلط بين البنك والنقد في المؤشرات — اختيار الحساب واضح في الشاشة.

### Completion Evidence
—

### Completion Summary
—

---

## Phase 13 — Inventory Counting & Adjustments

Status: Not Started

### Objective
التسويات الموثقة، الجرد بلقطة واعتماد الفروقات، والتحويل بين المخازن.

### Dependencies
P7. (يمكن التوازي مع P8–P12.)

### Tasks
- [ ] 13.1 `AdjustmentService` (BR-070) + تفعيل لوحة «تسوية مخزنية» في UI-07.
- [ ] 13.2 `InventoryCountService` (BR-071): فتح بلقطة، حفظ مرحلي، إرسال، اعتماد بتسوية مرتبطة، إلغاء، تقرير الفروقات.
- [ ] 13.3 `InventoryCountView` (S13): جدول العد، المسح المتكرر بالباركود للزيادة، مؤشرات التقدم، الفروقات بالقيمة.
- [ ] 13.4 `StockTransferService` (BR-072) + `StockTransferDialog` (S14).
- [ ] 13.5 قائمة «عمليات المخزون» في UI-07 (تسوية، جرد جديد، تحويل، أرصدة افتتاحية).
- [ ] 13.6 اختبارات TS-601…TS-603.
- [ ] 13.7 مراجعة بصرية UI-07 كاملة.

### Related UI
UI-07.

### Implementation
`Application/Inventory/{AdjustmentService, InventoryCountService, StockTransferService}`، `Desktop/Features/Inventory/*`.

### Database Changes
لا تغيير متوقع.

### Testing
TS-601…TS-603.

### Acceptance Criteria
- القاعدة الحرجة 12 مثبتة؛ الجرد مع بيع متزامن يعطي النتيجة الصحيحة.

### Risks
- جرد كبير (آلاف الأصناف) — حفظ مرحلي وترقيم.

### Completion Evidence
—

### Completion Summary
—

---

## Phase 14 — Dashboard & Reports

Status: Not Started

### Objective
لوحة التحكم (UI-02) ومركز التقارير (UI-19 وما يتبعها) بأرقام حقيقية قابلة للمطابقة، والبحث العام والتنبيهات.

### Dependencies
P9، P10، P11، P12، P13.

### Tasks
- [ ] 14.1 مولد بيانات تجريبية/أداء (Debug فقط، عبر خدمات Application، قاعدة `Mizad_Demo`/`Mizad_Perf` منفصلة) بحجم TEST_PLAN §5.
- [ ] 14.2 `DashboardService` + `DashboardView` (UI-02) مع DV-21.
- [ ] 14.3 بنية التقارير: تعريف التقرير، المرشحات، النتيجة (مؤشرات، سلاسل رسوم، جدول مُرقّم، إجماليات)، تطبيق BR-101 (حذف أعمدة الربح لا إخفاؤها).
- [ ] 14.4 التقارير: R01 المبيعات (تفصيلي/ملخص)، R02 المبيعات حسب الصنف والأكثر مبيعاً، R03 حسب العميل/المستخدم/طريقة الدفع، R04 المشتريات، R05 الأرباح (إجمالي وصافي)، R06 المخزون والتقييم، R07 حركة صنف، R08 منخفضة المخزون، R09 الراكدة، R10 ديون العملاء والأعمار، R11 مستحقات الموردين، R12 حركات الصندوق، R13 المصروفات، R14 المرتجعات (مع التالف)، R15 الورديات.
- [ ] 14.5 `ReportsView` (S21، DV-14): الفئات، الفترات، المرشحات، المؤشرات، الرسوم، الجدول وصف الإجماليات.
- [ ] 14.6 `GlobalSearchService` + `GlobalSearchPopup` (S30).
- [ ] 14.7 `NotificationService` + `NotificationsPopup` (D-038).
- [ ] 14.8 اختبارات TS-701…TS-708.
- [ ] 14.9 قياس أداء اللوحة والتقارير على بيانات 14.1.
- [ ] 14.10 مراجعة بصرية UI-02 وUI-19 (+UI-21/23/24 للمكونات المأخوذة).

### Related UI
UI-02، UI-18…UI-24.

### Implementation
`Application/Reporting/*`، `Desktop/Features/{Dashboard, Reports, Search}`، Views SQL عند الحاجة (توثق في DATABASE_DESIGN).

### Database Changes
محتملة: Views/فهارس للتقارير (ترحيل موثق).

### Testing
TS-701…TS-708؛ قياسات الأداء.

### Acceptance Criteria
- القواعد الحرجة 13، 14، 18 مثبتة؛ لا رقم ثابت؛ الأداء ضمن NFR.

### Risks
- بطء التجميعات على بيانات كبيرة — فهارس تغطية/Views.

### Completion Evidence
—

### Completion Summary
—

---

## Phase 15 — Printing & Exporting

Status: Not Started

### Objective
طباعة كل المستندات والتقارير على A4 والحرارية، المعاينة، PDF وExcel.

### Dependencies
P9 (للبدء)، P14 (للتقارير).

### Tasks
- [ ] 15.1 إعداد QuestPDF (الترخيص وفق قرار المالك D-022، تسجيل Tajawal، RTL) + Spike للعربية مبكراً (R-01).
- [ ] 15.2 `DocumentModel` وبانيات النماذج في Application لكل مستند.
- [ ] 15.3 القوالب: فاتورة بيع A4، إيصال حراري 80/58، فاتورة شراء A4، مرتجع A4/حراري، سند قبض/صرف، مصروف، كشف حساب، تقرير عام (عمودي/أفقي بمؤشرات وإجماليات)، ورقة جرد، تقرير وردية.
- [ ] 15.4 `PrintService`: تحويل الصفحات لصور بدقة الطابعة والطباعة، الطابعات لكل نوع من `machine.json`، النسخ.
- [ ] 15.5 `PrintPreviewWindow` (S27): تكبير، صفحات، طباعة، حفظ PDF.
- [ ] 15.6 `ExcelExporter` عام للقوائم والتقارير (ورقة RTL، تنسيق الأرقام، صف إجماليات).
- [ ] 15.7 تفعيل كل أزرار الطباعة/التصدير في الشاشات السابقة + الطباعة التلقائية بعد البيع (`Sales.AutoPrintReceipt`).
- [ ] 15.8 قسم «الطباعة والفواتير» في الإعدادات.
- [ ] 15.9 تمييز «نسخة» عند إعادة الطباعة + تدقيق.
- [ ] 15.10 اختبارات: محتوى PDF النصي (الرقم والإجماليات)، محتوى Excel، صلاحيات الطباعة؛ تجربة يدوية على طابعة حرارية (أو Microsoft Print to PDF عند عدم التوفر — يوثق).

### Related UI
UI-12 (قسم الطباعة)، أزرار الطباعة في UI-03/04/05/08/09/10/14/19.

### Implementation
`Application/Documents/*`، `Infrastructure/Documents/*`، `Desktop/Features/Printing/*`.

### Database Changes
لا شيء.

### Testing
انظر 15.10.

### Acceptance Criteria
- كل مستند قابل للطباعة والمعاينة والتصدير بعربية سليمة التشكيل.

### Risks
- R-01، R-11، R-13.

### Completion Evidence
—

### Completion Summary
—

---

## Phase 16 — Administration, Settings & Backup

Status: Not Started

### Objective
إدارة المستخدمين والأدوار كاملة، كل أقسام الإعدادات، النسخ اليدوي والمجدول، الاستعادة المحمية، سجل التدقيق والتشخيص.

### Dependencies
P4 (16.1–16.8 يمكن البدء بها مبكراً)، P15 (لإكمال الإعدادات)، كل المراحل للتشخيص.

### Tasks
- [ ] 16.1 `UsersView` (UI-11): المؤشرات، الجدول، إضافة/تعديل/تعطيل/إعادة تعيين، حماية آخر مدير.
- [ ] 16.2 الأدوار: القائمة، إنشاء/نسخ دور، مصفوفة الصلاحيات + الصلاحيات الخاصة (DV-16)، تحديث الجلسة (BR-102).
- [ ] 16.3 `SettingsView` (UI-12) بكل الأقسام: معلومات المحل (والشعار)، المبيعات، المشتريات، العملة والتنسيقات (القفل)، الطباعة (15.8)، النسخ، العامة، الترخيص والنسخة (معلومات الإصدار).
- [ ] 16.4 `BackupService`: **توسعة** خدمة النسخ الأساسية من 2.22: النسخ اليدوي من الواجهة، اكتشاف دعم الضغط، التسمية، شريط تقدم مع إلغاء (D-045).
- [ ] 16.5 التحقق من مسار النسخ وصلاحية حساب خدمة SQL مع إرشاد عربي (TS-905).
- [ ] 16.6 `BackupScheduler` (خدمة خلفية على مضيف النسخ) + تعويض الفائت عند الإقلاع.
- [ ] 16.7 الاحتفاظ بآخر N نسخة بنمط التسمية فقط.
- [ ] 16.8 `RestoreService` (D-021): الصلاحية، التحقق من الملف، تأكيد بكتابة «استعادة»، نسخة PreRestore، SINGLE_USER، REPLACE، فحوص بعد الاستعادة، إعادة التشغيل؛ رفض نسخة أحدث مخططاً.
- [ ] 16.9 ~~ربط نسخة PreMigration في `DatabaseInitializer`~~ (نُقل إلى 2.22–2.23 بقرار D-043). المتبقي: إظهار نسخ `PreMigration` في سجل النسخ وتطبيق سياسة الاحتفاظ عليها (لا تُحذف آخر نسخة PreMigration أبداً).
- [ ] 16.10 `BackupHistoryDialog` و`RestoreDialog` (S28).
- [ ] 16.11 `AuditLogView` (S29) بمرشحات المستخدم والتاريخ والإجراء.
- [ ] 16.12 شاشة تشخيص للمدير: فحوص مطابقة المخزون والتسويات والصندوق وإعادة احتساب `SettledAmount`.
- [ ] 16.13 اختبارات TS-901…TS-907 + ViewModels.
- [ ] 16.14 مراجعة بصرية UI-11 وUI-12 (مع DV-11، DV-17).

### Related UI
UI-11، UI-12.

### Implementation
`Application/{Identity, Administration}/*`، `Infrastructure/Backup/*`، `Desktop/Features/{Users, Settings, Backup, Audit, Diagnostics}`.

### Database Changes
لا تغيير متوقع.

### Testing
TS-901…TS-907.

### Acceptance Criteria
- نسخة → تحقق → استعادة إلى قاعدة منفصلة تُطابق البيانات؛ الاستعادة لا تتم دون كل الحمايات.

### Risks
- R-04، R-05، R-06.

### Completion Evidence
—

### Completion Summary
—

---

## Phase 17 — Integration Testing & Quality Assurance

Status: Not Started

### Objective
تحقق شامل وظيفي وتزامني وأدائي وأمني وبصري قبل الإصدار.

### Dependencies
P1–P16.

### Tasks
- [ ] 17.1 سيناريوهات E2E-01…E2E-08 (آلية حيث أمكن، يدوية موثقة غير ذلك).
- [ ] 17.2 حزمة التزامن: 5 مهام × 200 فاتورة على أصناف مشتركة + مرتجعات وتحصيلات متزامنة (NFR-08).
- [ ] 17.3 حزمة الأداء على بيانات 14.1 وتعبئة جدول TEST_PLAN §5.
- [ ] 17.4 المراجعة البصرية لكل شاشة مقابل صورتها (قائمة §7) بلقطات.
- [ ] 17.5 قابلية الاستخدام: نقطة البيع بلوحة المفاتيح فقط، ترتيب Tab، رسائل الأخطاء، الحالات الفارغة والتحميل، الدقات 1366×768 و125%/150%.
- [ ] 17.6 مراجعة الأمان: التفويض لكل الخدمات (TS-053 كامل)، لا SQL مركب من نصوص، لا أسرار في المستودع والسجلات والحزمة، القفل، حماية الاستعادة، تغطية التدقيق.
- [ ] 17.7 فحوص سلامة البيانات التشخيصية على بيانات الأداء.
- [ ] 17.8 تقرير التغطية (NFR-15) وسد الفجوات الحرجة.
- [ ] 17.9 فرز وإصلاح الأخطاء؛ توثيق المعروف المتبقي في §18.
- [ ] 17.10 حزمة المرونة (D-046): إيقاف خدمة SQL / قطع الشبكة / `KILL` للجلسات أثناء: التصفح، البحث، اعتماد بيع، تحصيل، تقرير، نسخ احتياطي ⇐ لا تجمّد، لا فقدان إدخال، لا تكرار، مؤشر الحالة صحيح، العودة التلقائية (E2E-09).
- [ ] 17.11 حزمة الترقية (D-043): ترقية من لقطة كل إصدار سابق إلى الحالي ببيانات الأداء، مع 3 أجهزة تُقلع معاً؛ قياس زمن الترحيل والنسخة المسبقة (E2E-08).

### Related UI
الكل.

### Implementation
`tests/*`، إصلاحات.

### Database Changes
حسب نتائج الأداء (موثقة).

### Testing
كل TS وE2E وNFR.

### Acceptance Criteria
- كل الاختبارات تمر؛ NFR محققة ومقاسة؛ لا أخطاء حرجة أو عالية مفتوحة.

### Risks
- اكتشاف مشكلات تصميمية متأخرة — المراجعة المستمرة بعد كل مرحلة تقلل ذلك.

### Completion Evidence
—

### Completion Summary
—

---

## Phase 18 — Packaging, Deployment & Release

Status: Not Started

### Objective
مثبت Windows احترافي، تجربة تثبيت وتحديث على أجهزة نظيفة، وإصدار 1.0.0.

### Dependencies
P17.

### Tasks
- [ ] 18.1 الإصدار: مصدر واحد في `Directory.Build.props`، `CHANGELOG.md`، العرض في الدخول و«حول».
- [ ] 18.2 النشر Self-Contained ReadyToRun win-x64 (DEPLOYMENT §3).
- [ ] 18.3 مثبت Inno Setup عربي: وضع خادم (اكتشاف/تثبيت SQL Express، TCP/IP، الجدار) ووضع عميل؛ إلغاء التثبيت يحفظ البيانات. **بعد التثبيت يُشغَّل البرنامج فيفتح معالج الإعداد (D-044) مباشرة** بالنمط المختار في المثبت: خادم ⇐ إنشاء القاعدة على النسخة المثبتة؛ عميل ⇐ الاتصال بالخادم.
- [ ] 18.3a صقل شاشة البداية ومعالج الإعداد على أجهزة نظيفة: رسائل الأسباب الشائعة (الخدمة متوقفة، TCP/IP معطل، الجدار، اسم النسخة، صلاحيات الحساب)، وزر «نسخ تفاصيل الدعم».
- [ ] 18.4 حسابات SQL للتطبيق والإدارة بأقل صلاحيات (DEPLOYMENT §2.2) ضمن معالج الخادم.
- [ ] 18.5 «نسخ معلومات الدعم» في نافذة «حول».
- [ ] 18.6 توقيع الكود (إن اعتمد المالك).
- [ ] 18.7 اختبار على أجهزة نظيفة: Windows 10 22H2 و11، خادم + عميل عبر الشبكة، ترقية من نسخة سابقة (ترحيل تلقائي في شاشة البداية بعد نسخة PreMigration، والعميل الأقدم يُطلب منه التحديث)، إلغاء وإعادة تثبيت.
- [ ] 18.8 استكمال DEPLOYMENT.md ودليل مستخدم مختصر `docs/USER_GUIDE.md`.
- [ ] 18.9 (اختياري بقرار المالك D-034) الترخيص والتفعيل.
- [ ] 18.10 قائمة فحص الإصدار (DEPLOYMENT §9) ووسم `v1.0.0`.

### Related UI
LOGO (أيقونة المثبت).

### Implementation
`installer/Mizad.iss`، `scripts/publish.ps1`، `CHANGELOG.md`، `docs/USER_GUIDE.md`.

### Database Changes
لا شيء.

### Testing
تجارب التثبيت والتحديث الموثقة.

### Acceptance Criteria
- تثبيت نظيف يعمل في الوضعين على جهازين، والترقية تحافظ على البيانات، وكل بنود قائمة الإصدار محققة.

### Risks
- R-17 تحذير SmartScreen دون توقيع؛ اختلاف بيئات SQL لدى العملاء.

### Completion Evidence
—

### Completion Summary
—

---

## 14. المخاطر التقنية (السجل العام)

| # | الخطر | الأثر | الاحتمال | المعالجة | المرحلة |
|---|---|---|---|---|---|
| R-01 | تشكيل العربية في QuestPDF/SkiaSharp | عالٍ | متوسط | Spike مبكر 15.1؛ بديل FlowDocument | P15 |
| R-02 | LiveCharts2 مع RTL وخط Tajawal | متوسط | متوسط | Spike 4.13؛ بديل ScottPlot | P4 |
| R-03 | Deadlock مع عدة أجهزة | عالٍ | متوسط | ترتيب أقفال ثابت، تحديثات ذرية، إعادة محاولة، TS-003 | P7, P9 |
| R-04 | قيود SQL Express (لا Agent، لا ضغط، 50GB) | متوسط | مؤكد | جدولة داخل التطبيق، اكتشاف الإصدار | P16 |
| R-05 | صلاحيات مسار النسخ لحساب خدمة SQL | عالٍ | مرتفع | تحقق بنسخة اختبار وإرشاد | P16 |
| R-06 | الاستعادة مع أجهزة متصلة | عالٍ | متوسط | SINGLE_USER، تحذير، إعادة تشغيل | P16 |
| R-07 | فروق تقريب بين الواجهة والخادم | عالٍ | متوسط | حاسبة Domain واحدة لكليهما؛ الخادم يعيد الحساب | P8, P9 |
| R-08 | أداء المطابقة FIFO | متوسط | منخفض | فهرس البنود المفتوحة؛ معالجة المفتوح فقط | P11 |
| R-09 | تضخم النطاق | عالٍ | مرتفع | D-039، موافقة المالك | الكل |
| R-10 | أداء DataGrid مع RTL | متوسط | متوسط | Virtualization، ترقيم على الخادم | P4+ |
| R-11 | تنوع الطابعات الحرارية | متوسط | مرتفع | طباعة صور بدقة الطابعة، 58/80 | P15 |
| R-12 | اختلاف ساعات الأجهزة | متوسط | متوسط | ساعة خادم SQL | P2 |
| R-13 | ترخيص QuestPDF | متوسط | منخفض | قرار المالك؛ بديل | P15 |
| R-14 | الشعار نقطي فقط بنص كحلي | منخفض | مؤكد | إنتاج نسخ؛ طلب ملف متجهي | P4 |
| R-15 | ترحيل بيانات من أنظمة قديمة | متوسط | متوسط | استيراد الأصناف 1.0؛ البقية مؤجلة | P5 |
| R-16 | تعارض عمل الوكلاء المتعددين | متوسط | متوسط | AGENT_INSTRUCTIONS §7، فروع/Worktrees | الكل |
| R-17 | تحذير SmartScreen للمثبت غير الموقع | منخفض | مرتفع | توقيع الكود | P18 |
| R-18 | فشل ترحيل تلقائي على قاعدة عميل حقيقية | عالٍ | منخفض | نسخة PreMigration إلزامية، كل ترحيل في معاملة، قفل الترحيل، حراسة `HasPendingModelChanges`، منع الترحيلات المدمرة، اختبارات ترقية من لقطات الإصدارات (D-043) | P2, P17 |
| R-19 | شبكة محلية غير مستقرة/انقطاع الخادم | عالٍ | متوسط | مراقب الاتصال، تعطيل الكتابة، الحفظ المحلي للإدخال، التحقق بـ `ClientRequestId` (D-046) | P4, P9, P17 |

---

## 15. حالة كل مرحلة
انظر جدول §9 (يُحدّث فور تغير أي حالة).

## 16. ما تم تنفيذه فعلياً
- **Phase 0 (2026-11-07):** فحص البيئة والصور الـ24 والشعار؛ استخراج الألوان؛ إنشاء الوثائق السبع وملفات الإدارة؛ 42 قراراً؛ تصميم ~45 جدولاً؛ خطة 19 مرحلة. لم يُكتب كود.
- **Phase 1 (2026-11-07):** بنية الحل (Domain/Application/Infrastructure/Desktop + UnitTests/IntegrationTests)، إدارة مركزية للحزم، `.editorconfig`، استضافة عامة في WPF، Serilog بحجب الأسرار، معالجة أخطاء عامة عربية، إعدادات محلية (اتصال مشفر DPAPI، جهاز، مستخدم)، اختبارات معمارية وتكامل على SQL Server، `scripts/build.ps1`. 33/33 اختباراً ناجحاً، 0 تحذيرات.
- **Phase 2 (2026-11-07):** 45 جدولاً وفق DATABASE_DESIGN بترحيل `InitialCreate`؛ ترحيل تلقائي آمن (قفل، نسخة PreMigration متحقق منها، ترحيلات بتقارير تقدم، مزامنة بذور، تدقيق، رفض القاعدة الأحدث)؛ وحدة العمل، الترقيم بلا فجوات، ساعة الخادم، التدقيق، الإعدادات، ترجمة أخطاء SQL. 85/85 اختباراً، 0 تحذيرات.

## 17. ما تبقى من العمل
Phase 3 حتى Phase 18 بالكامل (انظر §9 و§10).

## 18. المشاكل المعروفة
| # | المشكلة | الأثر | الحالة |
|---|---|---|---|
| K-01 | قرارات مقترحة بانتظار المالك: D-013 العملة، D-014 الضريبة، D-016 المخزون السالب، D-022 ترخيص QuestPDF، D-035 المثبت، D-034 الترخيص | قد تغير افتراضيات؛ لا تمنع P1–P4 | مفتوح |
| K-02 | الشعار متوفر كصورة نقطية فقط بنص كحلي | يحتاج نسخة فاتحة للشريط الجانبي | مفتوح (4.4) |
| K-03 | عمل Phase 0 وPhase 1 غير ملتزم به في Git (الفرع `phase/01-foundation` محلياً؛ `master` + `origin` بلا تغيير) | يُفقد إن لم يُلتزم | مفتوح — بانتظار إذن المالك بالالتزام |
| K-04 | بقايا القالب `Mizad/bin`, `Mizad/obj` | — | **مغلق** (1.3: المجلد القديم أُزيل كاملاً) |
| K-05 | بعد نقل المشروع اختار Visual Studio `Mizad.Application` (مكتبة) كمشروع بدء فيفشل F5 | **مُصلح في المستودع (2026-11-07):** `DefaultStartup` على `Mizad.Desktop` + `launchSettings.json` + اختبار حراسة؛ على جهاز المالك يبقى اختيار قديم محفوظ في `.vs/…/.suo` يُصحح مرة واحدة (Set as Startup Project) | مغلق |

## 19. آخر التحديثات
| التاريخ | الوكيل/الجلسة | التحديث |
|---|---|---|
| 2026-11-07 | إصلاح | F5 في Visual Studio: `DefaultStartup` لـ `Mizad.Desktop` في `Mizad.slnx` (مُتحقق بمكتبة VS 2026)، `launchSettings.json` (Development)، `appsettings.Development.json`، 3 اختبارات حراسة؛ 88/88 |
| 2026-11-07 | Phase 2 | تنفيذ المهام 2.1–2.27؛ 85/85 اختباراً، 0 تحذيرات؛ انحرافات التصميم موثقة في DATABASE_DESIGN §13؛ Phase 2 = Completed |
| 2026-11-07 | تحديث الخطة (طلب المالك) | D-043 ترحيل تلقائي آمن (مهام 2.21–2.27)؛ D-044 شاشة البداية ومعالج الإعداد (4.14–4.16، 18.3/18.3a)؛ D-045 العمل في الخلفية ومؤشرات التحميل (4.22)؛ D-046 مراقبة الاتصال (4.23–4.25، 9.12، 17.10)؛ D-047 الالتزام بطلب المالك فقط وCo-authored-by باسم المطور |
| 2026-11-07 | Phase 1 | تنفيذ المهام 1.1–1.15؛ 33/33 اختباراً، 0 تحذيرات؛ D-003 وD-041 محدّثان؛ Phase 1 = Completed |
| 2026-11-07 | Phase 0 | إنشاء الخطة الكاملة والوثائق وملفات الإدارة؛ Phase 0 = Completed |

## 20. المرحلة أو المهمة التالية
**Phase 3 — المهمة 3.1:** كتالوج الصلاحيات (BUSINESS_RULES §9) ثم 3.2 → 3.10 بالترتيب.
قبل البدء: الأدوار والصلاحيات تُضاف إلى `DatabaseSeeder` (مزامنة البذور — لا ترحيل لازم إلا إن تغيرت الكيانات)؛ `ICurrentUser` الحالي مؤقت (`AnonymousCurrentUser`) ويُستبدل في 3.6؛ `IAuditLogger.Log(context, entry)` جاهز للاستخدام.

---

## ملحق: أوامر مرجعية
```powershell
dotnet build Mizad.slnx -c Release          # بناء
dotnet test --solution Mizad.slnx           # اختبارات (Microsoft.Testing.Platform — global.json)
dotnet run --project src/Mizad.Desktop      # تشغيل
./scripts/build.ps1                         # restore + build Release + كل الاختبارات
./scripts/build.ps1 -UnitOnly               # بلا اختبارات التكامل (لا يحتاج SQL Server)
dotnet ef migrations add <Name> --project src/Mizad.Infrastructure --startup-project src/Mizad.Infrastructure --output-dir Persistence/Migrations
sqlcmd -S . -E -C -Q "SELECT name FROM sys.databases"
```

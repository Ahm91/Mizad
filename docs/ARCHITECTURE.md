# المعمارية — مِيزاد (ARCHITECTURE.md)

> القرارات المرجعية: D-001…D-004, D-017, D-019, D-020, D-042. أي تغيير في الطبقات أو حدود الاعتماد يتطلب قراراً جديداً في DECISIONS.md.

---

## 1. نظرة عامة

```
┌────────────────────────── Mizad.Desktop (WPF, net10.0-windows) ──────────────────────────┐
│  Views (XAML) ◄─ DataTemplates ─► ViewModels (CommunityToolkit.Mvvm)                      │
│  Themes/Controls · Navigation · Dialogs · Printing UI · Composition Root (Generic Host)  │
└───────────────┬───────────────────────────────────────────────────┬──────────────────────┘
                │ يستدعي واجهات وخدمات                               │ يسجل التنفيذات فقط (DI)
┌───────────────▼────────────── Mizad.Application (net10.0) ───────┐ │
│  Use-case services · DTOs · Validators · Authorization · Posting │ │
│  Interfaces: IMizadDbContext, IInventoryEngine, IClock, ...      │ │
└───────────────┬──────────────────────────────────────────────────┘ │
                │                                                    │
┌───────────────▼────── Mizad.Domain (net10.0) ─────┐   ┌────────────▼──── Mizad.Infrastructure (net10.0-windows) ───┐
│ Entities · Enums · Pure calculators (pricing,     │◄──┤ EF Core DbContext/Configs/Migrations · SQL helpers ·      │
│ avg cost, rounding, return limits) · Result/Error │   │ Backup · Password hasher · DPAPI · QuestPDF · ClosedXML · │
└───────────────────────────────────────────────────┘   │ Settings files · Serilog setup                            │
                                                        └───────────────────────────────────────────────────────────┘
```

### قواعد الاعتماد (يتحقق منها اختبار معماري في `Mizad.UnitTests/Architecture`)
| المشروع | يعتمد على | ممنوع عليه |
|---|---|---|
| Mizad.Domain | لا شيء (BCL فقط) | EF Core، WPF، Infrastructure، أي حزمة خارجية |
| Mizad.Application | Domain، `Microsoft.EntityFrameworkCore` (الأساسية فقط)، FluentValidation، Microsoft.Extensions.*.Abstractions | `EntityFrameworkCore.SqlServer`، WPF، Infrastructure، QuestPDF، ClosedXML |
| Mizad.Infrastructure | Application، Domain، EF SqlServer، QuestPDF، ClosedXML، Serilog | WPF، Desktop |
| Mizad.Desktop | Application، Domain، Infrastructure (**للتسجيل في DI فقط** داخل `Composition/`) | استدعاء DbContext أو SQL مباشرة من View/ViewModel |

---

## 2. الوحدات (Modules)

كل طبقة مقسمة إلى مجلدات بنفس أسماء الوحدات:

| الوحدة | المسؤولية | الخدمات الرئيسية (Application) |
|---|---|---|
| Identity | المستخدمون، الأدوار، الدخول، الجلسة | `AuthService`, `UserService`, `RoleService`, `IAuthorizationService`, `SetupService` |
| Catalog | التصنيفات، الوحدات، الأصناف، الباركود، الاستيراد | `CategoryService`, `UnitService`, `ProductService`, `ProductLookupService`, `ProductImportService` |
| Inventory | المحرك، الأرصدة، الحركات، التسويات، الجرد، التحويل، المخازن | `IInventoryEngine`, `StockQueryService`, `AdjustmentService`, `InventoryCountService`, `StockTransferService`, `WarehouseService` |
| Parties | العملاء، الموردون، الأرصدة الافتتاحية، الدفتر والمطابقة، الكشوف | `CustomerService`, `SupplierService`, `IPartyLedger`, `ISettlementEngine`, `StatementService`, `OpeningBalanceService` |
| Sales | فواتير البيع ونقطة البيع | `SalesInvoiceService`, `SalesQueryService` |
| Purchasing | فواتير الشراء | `PurchaseInvoiceService`, `PurchaseQueryService` |
| Returns | مرتجعات البيع والشراء | `SalesReturnService`, `PurchaseReturnService` |
| Treasury | الحسابات النقدية، السندات، المصروفات، التحويلات، الورديات | `ICashLedger`, `VoucherService`, `ExpenseService`, `CashTransferService`, `ShiftService`, `CashQueryService` |
| Reporting | لوحة التحكم، التقارير، البحث العام، التنبيهات | `DashboardService`, `ReportService` (لكل تقرير معالج)، `GlobalSearchService`, `NotificationService` |
| Administration | الإعدادات، النسخ والاستعادة، الترحيل، التدقيق، التشخيص | `ISettingsService`, `BackupService`, `RestoreService`, `DatabaseInitializer`, `IAuditLogger`, `DiagnosticsService` |
| Documents | الطباعة والتصدير | `IDocumentRenderer`, `IPrintService`, `IExcelExporter` |

**قاعدة:** وحدة لا تكتب في جداول وحدة أخرى مباشرة؛ تستدعي واجهتها (مثلاً Sales تستدعي `IInventoryEngine` و`IPartyLedger` و`ICashLedger`).

---

## 3. هيكل المجلدات

```
src/
├── Mizad.Domain/
│   ├── Common/          Entity, AuditableEntity, DocumentBase, Result, Error, DomainException, Money helpers
│   ├── Catalog/ Inventory/ Parties/ Sales/ Purchasing/ Returns/ Treasury/ Identity/ Administration/
│   └── Calculations/    InvoiceCalculator, AverageCostCalculator, ReturnCalculator, Rounding, ArabicNormalizer, Ean13
├── Mizad.Application/
│   ├── Abstractions/    IMizadDbContext, IClock, ICurrentUser, IAuthorizationService, IUnitOfWork, IAuditLogger, ISettingsService
│   ├── <Module>/        Services, Dtos, Validators, Queries
│   └── DependencyInjection.cs
├── Mizad.Infrastructure/
│   ├── Persistence/     MizadDbContext, Configurations/, Migrations/, Seed/, Interceptors/, SqlHelpers/
│   ├── Inventory/       InventoryEngine (SQL الذري)
│   ├── Security/        Pbkdf2PasswordHasher, DpapiProtector
│   ├── Backup/          SqlBackupService
│   ├── Documents/       QuestPdfRenderer, Templates/, ClosedXmlExporter
│   ├── Configuration/   ConnectionSettingsStore, MachineSettingsStore, UserPreferencesStore
│   └── DependencyInjection.cs
└── Mizad.Desktop/
    ├── App.xaml(.cs)    Host build, startup pipeline, global exception handling
    ├── Composition/     ServiceRegistration (يجمع DI لكل الطبقات)
    ├── Assets/          Fonts/Tajawal-*.ttf, Images/, Logo variants, app.ico
    ├── Themes/          Colors.xaml, Typography.xaml, Icons.xaml, Controls/*.xaml, Theme.xaml
    ├── Controls/        StatCard, StatusBadge, SearchBox, QuantityStepper, Pagination, ToggleSwitch, LoadingOverlay, EmptyState ...
    ├── Shell/           MainWindow, SidebarViewModel, HeaderViewModel, NavigationService, DialogService, ToastService
    ├── Features/<Module>/  Views + ViewModels لكل شاشة
    ├── Converters/  Behaviors/  Localization/ (MizadCulture)
    └── app.manifest     PerMonitorV2 DPI
tests/
├── Mizad.UnitTests/         Domain calculators, Application (بلا DB)، ViewModels، Architecture
└── Mizad.IntegrationTests/  SqlServerFixture، اختبارات الخدمات على SQL Server حقيقي
```

---

## 4. الأنماط الأساسية

### 4.1 MVVM
- `ObservableObject` / `[ObservableProperty]` / `[RelayCommand]` من CommunityToolkit.Mvvm.
- ViewModel-first: `INavigationService.NavigateTo<TViewModel>(param)`؛ الربط View↔ViewModel عبر `DataTemplate` في `Shell/ViewMappings.xaml`.
- Code-behind مسموح فقط لسلوك عرض بحت (تركيز، تمرير، رسوم). **ممنوع** أي منطق أعمال أو وصول للبيانات فيه.
- كل أمر غير متزامن `AsyncRelayCommand` مع `AllowConcurrentExecutions = false` و`IsBusy` لتعطيل الأزرار (BR-004).
- التحقق في الواجهة: `INotifyDataErrorInfo` يُغذى من نتائج المدققات (FluentValidation) الموجودة في Application — مصدر واحد للقواعد.

### 4.2 خدمات Application (Use Cases)
- كل عملية = دالة async في خدمة: `Task<Result<T>> PostAsync(PostSalesInvoiceCommand cmd, CancellationToken ct)`.
- التسلسل الثابت: **تفويض ← تحقق ← فتح سياق/معاملة ← تنفيذ ← تدقيق ← حفظ ← إرجاع Result**.
- الأخطاء المتوقعة (رصيد غير كافٍ، تجاوز حد…) تُرجع `Result.Failure(Error)` برموز ثابتة ورسائل عربية؛ الاستثناءات للأخطاء غير المتوقعة فقط.
- الاستعلامات للقراءة: `AsNoTracking` + Projection إلى DTO + ترقيم صفحات على الخادم.

### 4.3 خط الاعتماد (Posting Pipeline)
```
PostAsync(cmd)
 ├─ authz.Demand(Permission)
 ├─ validator.Validate(cmd)
 └─ unitOfWork.ExecuteInTransactionAsync(async ctx => {
       doc = load draft WITH rowversion check (or create)
       if doc.ClientRequestId already posted → return existing (idempotent)
       recompute totals (Domain calculator)          // لا ثقة بأرقام الواجهة
       business checks (discount/credit/below-cost)
       number = sequences.Next(docType)               // UPDLOCK
       inventoryEngine.Apply(stockPosting)            // ذري، مرتب
       partyLedger.Post(entries) + settlement.Explicit(...) + settlement.Fifo(party)
       cashLedger.Post(tx)                            // فحص السالب UPDLOCK
       audit.Log(...)
       doc.MarkPosted(number, serverClock.Now)
       SaveChanges
    })
```

### 4.4 إدارة DbContext في WPF
- `AddDbContextFactory<MizadDbContext>` — سياق جديد لكل حالة استخدام، يُتخلص منه فوراً (D-004).
- لا تتبع كيانات عبر الشاشات؛ الشاشة تحمل DTOs/نماذج تحرير، والخدمة تعيد التحميل وتطبق التغييرات مع `RowVersion` للتزامن المتفائل. تعارض ⇒ رسالة «عُدل هذا السجل من مستخدم آخر، أعد التحميل».

### 4.5 الخيوط (Threading)
- كل وصول لقاعدة البيانات async؛ ممنوع `.Result`/`.Wait()`.
- الـ ViewModel يعود للـ UI Thread تلقائياً بعد `await` (SynchronizationContext)؛ المعالجة الثقيلة (توليد PDF، استيراد Excel) في `Task.Run`.
- مؤشر تحميل لأي عملية > 300ms؛ إلغاء البحث السابق عند الكتابة (`CancellationToken` + Debounce 250ms).

### 4.6 الأمان
- `ICurrentUser` (Singleton للجلسة) يحمل المستخدم والصلاحيات؛ `IAuthorizationService.Demand` في بداية كل خدمة (BR-100).
- `SidebarViewModel` والأزرار تستعلم `HasPermission` لإخفاء/تعطيل العناصر — طبقة تجربة لا أمان.

### 4.7 الأخطاء والتسجيل
- Serilog → `%LocalAppData%\Mizad\logs\mizad-YYYYMMDD.log` (احتفاظ 30 يوماً) + Enrichers (Machine, User, Version).
- معالجات عامة: `DispatcherUnhandledException`، `TaskScheduler.UnobservedTaskException`، `AppDomain.UnhandledException` ⇒ تسجيل + نافذة عربية ودية برقم مرجعي (CorrelationId) دون تفاصيل حساسة.
- لا تُسجل كلمات المرور أو سلاسل الاتصال (سياسة Destructuring تحجبها).

### 4.8 الإعدادات (D-042)
| الملف/المكان | المحتوى |
|---|---|
| `appsettings.json` (بجانب البرنامج) | افتراضيات غير سرية (مستوى التسجيل، مهلات) |
| `%ProgramData%\Mizad\connection.json` | الخادم، القاعدة، نوع المصادقة، اسم مستخدم SQL، كلمة المرور مشفرة DPAPI |
| `%ProgramData%\Mizad\machine.json` | الطابعات، المخزن والصندوق الافتراضيان، مضيف النسخ |
| `%LocalAppData%\Mizad\user.json` | آخر اسم مستخدم، تفضيلات العرض |
| جدول `Settings` | إعدادات المحل المشتركة (BUSINESS_RULES §13) — `ISettingsService` مع ذاكرة مؤقتة تُحدّث عند الحفظ |

### 4.9 تسلسل الإقلاع
```
App.OnStartup
 → Build Host (DI, Serilog, Options) → SplashWindow (الشعار، الإصدار، الخطوة الجارية، شريط التقدم)
 → StartupOrchestrator (في الخلفية — D-044):
    1. ConnectionSettings موجودة؟ لا ⇒ معالج الإعداد (خادم محلي / خادم في الشبكة) ثم العودة
    2. اختبار الاتصال (مهلة 15 ث، 3 محاولات) — فشل ⇒ لوحة السبب: إعادة المحاولة / إعدادات الاتصال / نسخ تفاصيل الدعم / خروج
    3. DatabaseInitializer (تلقائي بالكامل — D-043):
         غير موجودة  ⇒ إنشاء (Arabic_100_CI_AI, RCSI) + Migrate + Seed
         ترحيلات معلقة ⇒ sp_getapplock ⇒ إعادة فحص ⇒ نسخة PreMigration متحقق منها ⇒ Migrate ⇒ Seed Sync ⇒ تحقق ⇒ تحرير القفل
         جهاز آخر يرحّل ⇒ «جارٍ تحديث قاعدة البيانات من جهاز آخر…» ثم المتابعة
         أحدث من البرنامج ⇒ رسالة منع «حدّث مِيزاد على هذا الجهاز»
    4. Users فارغ؟ ⇒ معالج التهيئة (المحل، العملة، المدير)
    5. تحميل الإعدادات المشتركة
 → LoginView → MainWindow (Shell) → لوحة التحكم
 → خدمات خلفية: ConnectionMonitor، BackupScheduler (إن كان الجهاز مضيف النسخ)، NotificationPoller
```
- حساب SQL للتطبيق يحتاج `db_owner` على قاعدة مِيزاد فقط لتطبيق الترحيلات والنسخ؛ الإنشاء الأول يحتاج `dbcreator` (يُستخدم في معالج الإعداد).

### 4.10 الطباعة والتصدير (D-022)
- `IDocumentRenderer.Render(DocumentModel, Format)` → PDF bytes أو صور صفحات.
- `IPrintService.Print(DocumentModel, PrinterKind)` يختار الطابعة من `machine.json` ويطبع الصور بدقة الطابعة. **التنفيذ في Mizad.Desktop** لأنه يستخدم طباعة WPF، واختبار المعمارية يمنع WPF في Infrastructure.
- النماذج (DocumentModel) تُبنى في Application من البيانات؛ القوالب في Infrastructure.

---

### 4.11 العمليات في الخلفية ومؤشرات التحميل (D-045)
```csharp
// كل أمر يمس القاعدة يمر عبر هذا النمط — لا استدعاء مباشر من معالج حدث
[RelayCommand(AllowConcurrentExecutions = false)]
private Task SaveAsync(CancellationToken ct) =>
    RunAsync(async token => { var result = await _service.SaveAsync(Model, token); ... },
             BusyScope.Button /* أو Region / Page / LongRunning */, ct);
```
- `ViewModelBase.RunAsync`: يضبط حالة الانشغال (`IsBusy`/`IsLoading`) ⇐ يسجل العملية في `IBusyTracker` (الشريط العام في الترويسة) ⇐ يطبق المهلة والإلغاء ⇐ يحول `Result.Failure` لرسالة/إشعار عربي ⇐ يحول أخطاء الاتصال إلى `IConnectionMonitor.ReportFailure` ورسالة «تعذر الاتصال بقاعدة البيانات…» ⇐ يسجل غير المتوقع برقم مرجعي.
- النطاقات: `Button` (مؤشر داخل الزر + تعطيله)، `Region` (`LoadingOverlay` على البطاقة/الجدول)، `Page` (تحميل الصفحة الأول بهيكل/Skeleton)، `LongRunning` (> 2 ث: شريط تقدم + نسبة + إلغاء).
- المهل الافتراضية: أوامر 30 ث، تقارير 120 ث، النسخ الاحتياطي بلا مهلة مع تقدم. البحث: Debounce 250 مللي ث وإلغاء الطلب السابق.

### 4.12 مرونة الاتصال (D-046)
```
            فشل عملية عابر / فشل الفحص
 Connected ─────────────────────────────► Reconnecting ──(فشل متكرر)──► Disconnected
     ▲                                         │                            │
     └──────────── نجاح فحص SELECT 1 ◄─────────┴────────────────────────────┘
                    (تراجع 2، 4، 8 … 30 ث)
```
- `IConnectionMonitor` (Application) / `ConnectionMonitor` (Infrastructure): مصنّف أخطاء SqlException العابرة (المهلة -2، الشبكة 53/40/64/121/233/10053/10054/10060/10061، تعذر فتح القاعدة 4060…)، فحص دوري كل 30 ث، حدث `StateChanged`.
- الواجهة: `ConnectionIndicator` في الترويسة، `OfflineBanner` أعلى المحتوى، إشعار عند العودة؛ أوامر الكتابة `CanExecute = IsConnected`.
- الإدخال لا يضيع: `IRecoveryStore` يحفظ مسودات الإدخال الطويلة (سلة نقطة البيع، الفواتير قيد الإنشاء) محلياً.
- العمليات المالية المقطوعة: التحقق بـ `ClientRequestId` (D-011) بعد العودة قبل أي إعادة.

## 5. اصطلاحات الكود
- C# أحدث إصدار، `Nullable` مفعل، `TreatWarningsAsErrors` في src، `AnalysisLevel=latest-recommended`، `.editorconfig` موحد.
- أسماء الكود بالإنجليزية؛ النصوص الظاهرة للمستخدم بالعربية في ملفات موارد/ثوابت لكل وحدة (`Strings.Sales.cs`) لا مبعثرة.
- `CancellationToken` في كل API غير متزامن عام.
- لا `async void` إلا لمعالجات الأحداث.
- كل خدمة Application لها واجهة لتسهيل اختبار ViewModels.
- الاستعلامات الخام عبر `FromSql`/`ExecuteSql` بمعاملات فقط — **ممنوع** تركيب SQL بالنصوص من مدخلات المستخدم.
- لا شيفرة ميتة ولا TODO بلا مهمة مسجلة في agent.md.

## 6. نقاط التوسع المستقبلية
- تعدد العملات: إضافة `CurrencyId` و`ExchangeRate` للمستندات (المبالغ الحالية تصبح بالعملة الأساسية).
- الفروع: الاعتماد على `WarehouseId` الموجود + جدول Branches.
- الويب/الجوال: طبقة Application مستقلة عن WPF تسمح بإضافة API لاحقاً.
- الترخيص: `ILicenseService` في Administration (D-034).

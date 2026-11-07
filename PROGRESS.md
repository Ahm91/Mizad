# PROGRESS.md — آخر حالة فعلية لمشروع «مِيزاد»

> يُحدّث بعد كل إنجاز أو تغير مهم. اقرأه بعد `AGENT_INSTRUCTIONS.md` و`agent.md`.

**آخر تحديث:** 2026-11-07 · **حالة الكود:** يبني ✅ (0 تحذيرات) · الاختبارات ✅ 88/88

---

## الحالة الحالية
| البند | القيمة |
|---|---|
| المرحلة الحالية | **Phase 3 — Authentication & Security Foundation** — Not Started |
| آخر مرحلة مكتملة | Phase 2 — Database Design & Infrastructure (2026-11-07) |
| آخر مهمة مكتملة | 2.27 توثيق سياسة الترحيلات + التحقق النهائي للمرحلة |
| المهمة التالية | **3.1** كتالوج الصلاحيات `Permissions` (BUSINESS_RULES §9) |
| فرع Git الحالي | `phase/02-database` (متفرع من `phase/01-foundation`). **عمل Phase 2 غير ملتزم به** — الالتزام بطلب المالك فقط (D-047) |
| التزامات موجودة | `phase/01-foundation`: `8c7e692` (وثائق Phase 0)، `08ee566` (أساس Phase 1) — بهوية Ahm91. `master` و`origin` بلا تغيير، لا دفع |
| العمل الجاري | لا شيء |
| مسؤول دمج الملفات المشتركة | غير محدد (وكيل واحد حالياً) |

## آخر عملية بناء
| التاريخ | الأمر | النتيجة |
|---|---|---|
| 2026-11-07 | `./scripts/build.ps1` (restore ⇐ build Release ⇐ `dotnet test --solution Mizad.slnx`) | Build succeeded — **0 Warning(s), 0 Error(s)** — 39.6 ث |

## الاختبارات المنفذة
| التاريخ | الأمر | النتيجة |
|---|---|---|
| 2026-11-07 | `dotnet test --solution Mizad.slnx -c Release` | **total 85 · succeeded 85 · failed 0 · skipped 0** — وحدات 52 + تكامل 33 على SQL Server 2025 المحلي |
| 2026-11-07 | تشغيل `Mizad.exe` (Release) | يُقلع ويستجيب ويُغلق برمز 0 |
| 2026-11-07 | فحص النظافة بعد الاختبارات | 0 قواعد `Mizad_IT_*`، 0 سجلات نسخ في msdb، 0 ملفات نسخ اختبار |

## الملفات المعدلة في آخر جلسة (Phase 2 — غير ملتزم بها)
- **جديدة:** `dotnet-tools.json` (dotnet-ef 10.0.14)؛ كيانات Domain لكل الوحدات (`Identity`، `Administration`، `Catalog`، `Inventory`، `Parties`، `Sales`، `Purchasing`، `Returns`، `Treasury`)، `Common/DocumentEntity.cs`، `Common/SharedEnums.cs`، `Calculations/ArabicNormalizer.cs`، `Administration/SettingKeys.cs`؛ `Application/Abstractions/{Persistence, Database}`؛ `Infrastructure/Persistence/**` (السياق، الإعدادات، الخدمات، المُهيئ، القفل، البذّار، الترحيل `InitialCreate`)، `Infrastructure/Backup/DatabaseBackupService.cs`؛ اختبارات `TestDatabase`، `TestData`، `Persistence/*`، `ArabicNormalizerTests`، `SettingsAndSequencesTests`، `ModelAndMigrationGuardTests`.
- **معدلة:** `Directory.Packages.props`، csproj (Application، Infrastructure، IntegrationTests)، `.editorconfig`، `Common/Entity.cs`، `IAuditLogger.cs`، `Infrastructure/DependencyInjection.cs`، `SqlServerFixture.cs`.
- **وثائق:** `agent.md`، `DECISIONS.md` (ملاحظة تنفيذ D-043)، `docs/DATABASE_DESIGN.md` (§13 الانحرافات، §14 الأوامر)، `docs/TEST_PLAN.md`، `docs/DEPLOYMENT.md`، `AGENT_INSTRUCTIONS.md` (أمر الترحيل)، `PROGRESS.md`.

## المشاكل المفتوحة
| # | المشكلة | الأثر | المهمة |
|---|---|---|---|
| K-01 | قرارات «مقترحة» بانتظار تأكيد المالك (انظر أدناه) | لا تمنع P3–P4 | — |
| K-02 | الشعار نقطي فقط ونصه كحلي (لا يظهر على الشريط الكحلي) | يحتاج نسخة فاتحة | 4.4 |
| K-03 | عمل Phase 2 غير ملتزم به | يُفقد إن لم يُلتزم | بانتظار طلب المالك |
| K-05 | اختيار مشروع البدء القديم محفوظ في `.vs/Mizad.slnx/v18/.suo` على جهاز المالك | F5 يحاول تشغيل `Mizad.Application` حتى يُصحح مرة واحدة | المالك: زر أيمن على `Mizad.Desktop` ← Set as Startup Project (المستودع نفسه مُصلح) |

## القرارات المطلوبة من مالك المشروع
| # | السؤال | الافتراض الحالي (يُطبق ما لم يُغيّر) | يؤثر على |
|---|---|---|---|
| Q-01 | العملة الافتراضية ومنازلها؟ (التصاميم فيها ر.س ود.ع) | الدينار العراقي «د.ع» بـ 0 منازل، قابل للتغيير في معالج التهيئة — D-013 | P4 (المعالج)، P9 |
| Q-02 | هل تحتاج ضريبة قيمة مضافة؟ | معطلة افتراضياً، معدل واحد اختياري — D-014 | P9 |
| Q-03 | السماح بالبيع بالسالب؟ | ممنوع افتراضياً، إعداد قابل للتفعيل — D-016 | P7 |
| Q-04 | ترخيص QuestPDF: هل الدخل السنوي لشركتكم أقل من مليون دولار (رخصة Community مجانية)؟ | نعم (Community) — D-022 | P15 |
| Q-05 | المثبت Inno Setup وتوقيع الكود بشهادة؟ | Inno Setup، بلا توقيع حتى توفر شهادة — D-035 | P18 |
| Q-06 | نظام الترخيص والتفعيل: مطلوب في 1.0؟ وبأي نموذج؟ | مؤجل — D-034 | P18 |
| Q-07 | هل يتوفر الشعار بصيغة متجهية (SVG/AI) أو نسخة بنص أبيض؟ | سننتج نسخة من PNG | 4.4 |
| ~~Q-08~~ | ~~سياسة الالتزام في Git~~ | **محسوم (D-047):** لا التزام تلقائي؛ الالتزام والدفع بطلب المالك فقط؛ `Co-authored-by: Ahm91 <ahmed.been8@gmail.com>`؛ لا أسطر نسبة لأدوات الذكاء الاصطناعي | — |

## الخطوات التالية
1. (المالك) طلب الالتزام لعمل Phase 2 متى شاء؛ ودمج الفروع في `master` والدفع إلى `origin` بطلب منفصل.
2. بدء Phase 3 بالمهام 3.1 → 3.10 بالترتيب.

## متطلبات المالك المضافة (2026-11-07) — أين وُضعت
| المتطلب | القرار | المهام | الحالة |
|---|---|---|---|
| الالتزام بطلب المالك فقط + Co-authored-by باسم المطور | D-047 | AGENT_INSTRUCTIONS §6 و§9 و§11 | مطبق |
| ترحيلات تلقائية صحيحة بلا مشاكل، تستوعب أي تعديل مستقبلي | D-043 | 2.14، 2.21–2.27 ✅؛ 16.9، 17.11، 18.7 | **منفذ ومختبر في P2** |
| شاشة بداية تعالج كل شيء + معالج إعداد للتنصيب الجديد | D-044 | 4.14–4.16، 18.3، 18.3a | P4 (الأساس جاهز: `IDatabaseInitializer` بتقارير تقدم عربية) |
| العمليات في الخلفية + مؤشر تحميل لكل عملية | D-045 | 4.22 | P4 |
| إظهار حالة الاتصال وسلوك آمن عند الانقطاع | D-046 | 4.23–4.26، 9.12، 17.10 | P4 (الأساس جاهز: `SqlErrorTranslator.IsConnectivityError`) |

## تعليمات الاستكمال للوكيل التالي
1. نفّذ §1 من `AGENT_INSTRUCTIONS.md`. تحقق أن `./scripts/build.ps1` ينجح (85/85) قبل أي تعديل. يحتاج SQL Server محلياً أو `MIZAD_TEST_SQL`.
2. غيّر حالة Phase 3 إلى `In Progress` في `agent.md` (العنوان + جدول §9 + الجدول العلوي).
3. **ملاحظات Phase 3:**
   - الأدوار الأربعة وصلاحياتها (3.2) تُضاف إلى `src/Mizad.Infrastructure/Persistence/Seed/DatabaseSeeder.cs` بأسلوب «أضف الناقص فقط»؛ دور المدير يأخذ كل الصلاحيات **بما فيها الجديدة مستقبلاً** في كل مزامنة (D-043 بند 3).
   - `ICurrentUser` حالياً `AnonymousCurrentUser` مسجّل بـ `TryAddSingleton` في `AddInfrastructure` — سجّل التنفيذ الحقيقي (3.6) قبله أو استبدله.
   - `IAuditLogger.Log(context, entry)` يضيف سطراً للسياق؛ المستدعي يحفظ داخل نفس المعاملة (`IUnitOfWork.ExecuteInTransactionAsync`).
   - كيان `User` جاهز (PasswordHash varchar(200)، FailedLoginCount، LockoutEndUtc، MustChangePassword)؛ إنشاء المدير الأول (3.8) يجب أن يفحص فراغ الجدول داخل معاملة Serializable.
   - أي تعديل على الكيانات = ترحيل: `dotnet ef migrations add <Name> --project src/Mizad.Infrastructure --startup-project src/Mizad.Infrastructure --output-dir Persistence/Migrations` ثم اختبار `Model_HasNoChangesWithoutAMigration`.
4. **ملاحظات تقنية متراكمة:**
   - الاختبارات على **Microsoft.Testing.Platform**: `dotnet test --solution Mizad.slnx`. استخدم `TestContext.Current.CancellationToken` في كل استدعاء async داخل الاختبارات.
   - اختبارات التكامل: `fixture.Database` (`TestDatabase`) قاعدة مشتركة جاهزة بالترحيلات والبذور؛ للحالات الخاصة أنشئ `new TestDatabase()` خاصاً (يُحذف مع نسخه تلقائياً).
   - **D-041:** البحث العربي يقارن `SearchName` بـ `ArabicNormalizer.Normalize(text)` — الترتيب لا يتجاهل التشكيل.
   - داخل `namespace Mizad.Desktop` استخدم `System.Windows.Application` صراحة.
   - مشاريع `UseWPF` لا تستورد `System.IO` ضمنياً. السكربتات `.ps1` ASCII فقط.
   - في T-SQL: `EXEC(...)` لا يقبل استدعاء دوال — ابنِ النص في متغير ثم `sp_executesql`. والاستعلام عن جدول قد لا يوجد يجب أن يكون في تعليمة منفصلة داخل `IF OBJECT_ID(...)`.

---

## سجل الجلسات (الأحدث أولاً)
| التاريخ | الجلسة | ما أُنجز | البناء | الاختبارات |
|---|---|---|---|---|
| 2026-11-07 | إصلاح F5 | `DefaultStartup="true"` على `Mizad.Desktop` في `Mizad.slnx` (السبب: VS 2026 يختار أول مشروع أبجدياً عند غياب الإعداد — `Mizad.Application`)؛ `launchSettings.json` + `appsettings.Development.json`؛ `SolutionFileTests` (3) | ✅ 0/0 | ✅ 88/88 |
| 2026-11-07 | Phase 2 | المهام 2.1–2.27: 45 كياناً وإعداداتها وقيودها، الترحيل `InitialCreate`، الترحيل التلقائي الآمن (قفل + نسخة مسبقة + تقدم + بذور + تدقيق)، وحدة العمل، الترقيم، ساعة الخادم، الإعدادات، ترجمة الأخطاء؛ Phase 2 = Completed | ✅ 0/0 | ✅ 85/85 |
| 2026-11-07 | Git | التزامان بطلب المالك على `phase/01-foundation` بهوية `Ahm91 <ahmed.been8@gmail.com>`؛ إنشاء `phase/02-database` | — | — |
| 2026-11-07 | تحديث الخطة | متطلبات المالك: D-043…D-047؛ مهام جديدة في P2 وP4 وP9 وP16 وP17 وP18؛ تحديث AGENT_INSTRUCTIONS والوثائق؛ حذف صلاحية `System.Migrate` | — (وثائق فقط) | — |
| 2026-11-07 | Phase 1 | المهام 1.1–1.15: بنية الحل، CPM، الاستضافة، Serilog، الإعدادات المحلية وDPAPI، الاختبارات المعمارية والتكامل، build.ps1؛ تحديث D-003 وD-041؛ Phase 1 = Completed | ✅ 0/0 | ✅ 33/33 |
| 2026-11-07 | Phase 0 | فحص البيئة والصور الـ24 والشعار؛ استخراج ألوان التصميم؛ إنشاء كل الوثائق وملفات الإدارة؛ Phase 0 = Completed | ✅ 0/0 (القالب) | لا يوجد |

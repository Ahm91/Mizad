# PROGRESS.md — آخر حالة فعلية لمشروع «مِيزاد»

> يُحدّث بعد كل إنجاز أو تغير مهم. اقرأه بعد `AGENT_INSTRUCTIONS.md` و`agent.md`.

**آخر تحديث:** 2026-11-07 · **حالة الكود:** يبني ✅ (0 تحذيرات) · الاختبارات ✅ 33/33

---

## الحالة الحالية
| البند | القيمة |
|---|---|
| المرحلة الحالية | **Phase 2 — Database Design & Infrastructure** — Not Started |
| آخر مرحلة مكتملة | Phase 1 — Solution Architecture & Project Foundation (2026-11-07) |
| آخر مهمة مكتملة | 1.15 التحقق النهائي (بناء + اختبارات + تشغيل) |
| المهمة التالية | **2.1** `MizadDbContext` + `IMizadDbContext` + `ConfigureConventions` + `UseCollation("Arabic_100_CI_AI")` |
| فرع Git الحالي | `phase/01-foundation` (محلي، **لا التزامات** — كل العمل غير ملتزم به) |
| العمل الجاري | لا شيء |
| مسؤول دمج الملفات المشتركة | غير محدد (وكيل واحد حالياً) |

## آخر عملية بناء
| التاريخ | الأمر | النتيجة |
|---|---|---|
| 2026-11-07 | `./scripts/build.ps1` (restore ⇐ `dotnet build Mizad.slnx -c Release` ⇐ `dotnet test --solution Mizad.slnx`) | Build succeeded — **0 Warning(s), 0 Error(s)** |

## الاختبارات المنفذة
| التاريخ | الأمر | النتيجة |
|---|---|---|
| 2026-11-07 | `dotnet test --solution Mizad.slnx -c Release` | **total 33 · succeeded 33 · failed 0 · skipped 0** — وحدات 26 (معمارية 6، Result 6، إعدادات محلية/DPAPI 7، سلسلة الاتصال 4، السجل 3) + تكامل 7 على SQL Server 2025 المحلي |
| 2026-11-07 | تشغيل يدوي `Mizad.exe` (Release) | يُقلع ويستجيب، يُغلق برمز 0، السجل `%LocalAppData%\Mizad\logs\mizad-20261107.log` صحيح |

## الملفات المعدلة في آخر جلسة (Phase 1)
- **جديدة:** `.editorconfig`، `global.json`، `Directory.Build.props`، `Directory.Packages.props`، `scripts/build.ps1`، `src/Mizad.Domain/**`، `src/Mizad.Application/**`، `src/Mizad.Infrastructure/**`، `src/Mizad.Desktop/Composition/*`، `src/Mizad.Desktop/app.manifest`، `src/Mizad.Desktop/appsettings.json`، `tests/Mizad.UnitTests/**`، `tests/Mizad.IntegrationTests/**`.
- **منقولة/معدلة:** `Mizad/*` ⇐ `src/Mizad.Desktop/*`؛ `Mizad.slnx`، `.gitignore`، `.gitattributes`.
- **وثائق:** `agent.md`، `DECISIONS.md` (D-003، D-041)، `docs/DATABASE_DESIGN.md` §1، `docs/TEST_PLAN.md`، `docs/DEPLOYMENT.md`، `docs/ARCHITECTURE.md` §4.10.

## المشاكل المفتوحة
| # | المشكلة | الأثر | المهمة |
|---|---|---|---|
| K-01 | قرارات «مقترحة» بانتظار تأكيد المالك (انظر أدناه) | لا تمنع P2–P4 | — |
| K-02 | الشعار نقطي فقط ونصه كحلي (لا يظهر على الشريط الكحلي) | يحتاج نسخة فاتحة | 4.4 |
| K-03 | عمل Phase 0 وPhase 1 كله غير ملتزم به في Git | يُفقد إن لم يُلتزم | بانتظار إذن المالك |
| K-05 | Visual Studio كان مفتوحاً على الحل أثناء نقل المشروع | قد يعرض `Mizad\Mizad.csproj` القديم كمفقود | أغلق الحل وأعد فتح `Mizad.slnx` |

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
| ~~Q-08~~ | ~~سياسة الالتزام في Git~~ | **محسوم (D-047):** لا التزام تلقائي أبداً؛ الالتزام والدفع بطلب المالك فقط؛ `Co-authored-by` باسم المطور؛ لا أسطر نسبة لأدوات الذكاء الاصطناعي | — |
| ~~Q-09~~ | ~~هوية المطور لسطر `Co-authored-by`~~ | **محسوم:** `Ahm91 <ahmed.been8@gmail.com>` (مسجلة في AGENT_INSTRUCTIONS §6) | — |

## الخطوات التالية
1. ~~هوية المطور~~ محسومة (Q-09). الالتزام والدفع بطلب المالك فقط (D-047).
2. بدء Phase 2 بالمهام 2.1 → 2.27 بالترتيب (2.21–2.27 الترحيل التلقائي الآمن — D-043).

## متطلبات المالك المضافة (2026-11-07) — أين وُضعت
| المتطلب | القرار | المهام |
|---|---|---|
| الالتزام بطلب المالك فقط + Co-authored-by باسم المطور | D-047 | AGENT_INSTRUCTIONS §6 و§9 و§11 |
| ترحيلات تلقائية صحيحة بلا مشاكل، تستوعب أي تعديل مستقبلي | D-043 | 2.14، 2.21–2.27، 16.9، 17.11، 18.7؛ DATABASE_DESIGN §14 |
| شاشة بداية تعالج كل شيء + معالج إعداد للتنصيب الجديد (إنشاء/اتصال) | D-044 | 4.14–4.16، 18.3، 18.3a |
| العمليات في الخلفية + مؤشر تحميل لكل عملية | D-045 | 4.22، تعريف «مكتملة» للشاشات |
| إظهار حالة الاتصال وسلوك آمن عند الانقطاع | D-046 | 4.23–4.26، 9.12، 17.10 |

## تعليمات الاستكمال للوكيل التالي
1. نفّذ §1 من `AGENT_INSTRUCTIONS.md`. تحقق أن `./scripts/build.ps1` ينجح قبل أي تعديل.
2. اقرأ D-043…D-047 في DECISIONS.md (متطلبات المالك الجديدة: الترحيل التلقائي، شاشة البداية، التحميل، الاتصال، سياسة Git) و§2.1–§2.2 و§6 في AGENT_INSTRUCTIONS.
3. غيّر حالة Phase 2 إلى `In Progress` في `agent.md` (عنوان المرحلة + جدول §9 + الجدول العلوي).
4. قبل 2.1: أضف إلى `Directory.Packages.props`: `Microsoft.EntityFrameworkCore`، `Microsoft.EntityFrameworkCore.SqlServer`، `Microsoft.EntityFrameworkCore.Design` (10.0.14 أو أحدث مستقر من سلسلة 10.0)؛ Application تأخذ `Microsoft.EntityFrameworkCore` فقط (D-004)؛ Infrastructure تأخذ SqlServer وDesign. حدّث اختبار المعمارية إن لزم (Application ممنوع عليها SqlServer — مطبق).
5. **ملاحظات تقنية من Phase 1 (مهمة):**
   - الاختبارات على **Microsoft.Testing.Platform**: `dotnet test --solution Mizad.slnx` (لا `dotnet test Mizad.slnx`). استخدم `TestContext.Current.CancellationToken` في كل استدعاء async داخل الاختبارات (محلل xUnit1051 يحوّلها لأخطاء).
   - اختبارات التكامل: `SqlServerFixture` (Assembly Fixture) يوفر `ConnectionString` لقاعدة `Mizad_IT_<guid>` فارغة بالترتيب العربي. في Phase 2 طبّق الترحيلات عليها داخل الـ Fixture.
   - **D-041:** SQL Server لا يتجاهل التشكيل العربي ولا يوحّد أ/ا أو ة/ه في أي ترتيب — كل بحث عربي يقارن `SearchName` المُطبَّع.
   - داخل `namespace Mizad.Desktop` استخدم `System.Windows.Application` صراحة (تعارض مع namespace `Mizad.Application`).
   - مشاريع `UseWPF` لا تستورد `System.IO` ضمنياً.
   - `IUnitOfWork` لم يُعرّف بعد — مكانه 2.15 بعد `IMizadDbContext`.
   - السكربتات `.ps1` ASCII فقط (Windows PowerShell 5.1 يقرأ UTF-8 دون BOM كـ ANSI).

---

## سجل الجلسات (الأحدث أولاً)
| التاريخ | الجلسة | ما أُنجز | البناء | الاختبارات |
|---|---|---|---|---|
| 2026-11-07 | تحديث الخطة | متطلبات المالك: D-043…D-047؛ مهام جديدة في P2 وP4 وP9 وP16 وP17 وP18؛ تحديث AGENT_INSTRUCTIONS والوثائق؛ حذف صلاحية `System.Migrate` | — (وثائق فقط) | — |
| 2026-11-07 | Phase 1 | المهام 1.1–1.15: بنية الحل، CPM، الاستضافة، Serilog، الإعدادات المحلية وDPAPI، الاختبارات المعمارية والتكامل، build.ps1؛ تحديث D-003 (منصة الاختبار، SqlClient 6.1.8) وD-041 (سلوك الترتيب العربي المُتحقق منه)؛ Phase 1 = Completed | ✅ 0/0 | ✅ 33/33 |
| 2026-11-07 | Phase 0 | فحص البيئة والصور الـ24 والشعار؛ استخراج ألوان التصميم بعينات بكسل؛ إنشاء كل الوثائق وملفات الإدارة؛ Phase 0 = Completed | ✅ 0/0 (القالب) | لا يوجد |

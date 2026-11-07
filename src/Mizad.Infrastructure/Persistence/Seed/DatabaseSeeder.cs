using Microsoft.EntityFrameworkCore;
using Mizad.Domain.Administration;
using Mizad.Domain.Catalog;
using Mizad.Domain.Inventory;
using Mizad.Domain.Parties;
using Mizad.Domain.Returns;
using Mizad.Domain.Treasury;

namespace Mizad.Infrastructure.Persistence.Seed;

/// <summary>
/// Seed sync (tasks 2.12, 2.24, D-043): inserts ONLY what is missing, matched by natural keys, and never modifies
/// existing rows — safe to run after every migration and on every start. No users are seeded (D-017).
/// Roles and permissions are added here in Phase 3 (task 3.2).
/// </summary>
internal static class DatabaseSeeder
{
    internal static readonly string[] UnitNames = ["حبة", "قطعة", "كجم", "غرام", "لتر", "علبة", "كرتون", "عبوة", "كيس", "متر"];

    internal static readonly (string Name, AdjustmentDirection Direction, bool IsSystem)[] AdjustmentReasons =
    [
        ("رصيد افتتاحي", AdjustmentDirection.In, true),
        ("زيادة جرد", AdjustmentDirection.In, false),
        ("نقص جرد", AdjustmentDirection.Out, false),
        ("تالف", AdjustmentDirection.Out, false),
        ("هدايا وعينات", AdjustmentDirection.Out, false),
        ("استهلاك داخلي", AdjustmentDirection.Out, false),
        ("تصحيح إدخال", AdjustmentDirection.Both, false),
    ];

    internal static readonly (string Name, ReturnReasonScope Scope, bool DefaultRestock)[] ReturnReasons =
    [
        ("عيب في المنتج", ReturnReasonScope.Both, false),
        ("بضاعة تالفة", ReturnReasonScope.Both, false),
        ("خطأ في الطلب", ReturnReasonScope.Both, true),
        ("انتهاء الصلاحية", ReturnReasonScope.Both, false),
        ("تغيير رأي العميل", ReturnReasonScope.Sales, true),
        ("أخرى", ReturnReasonScope.Both, true),
    ];

    internal static readonly (string Name, VoucherReasonScope Scope, ProfitEffect Effect)[] VoucherReasons =
    [
        ("إيداع مالك", VoucherReasonScope.Receipt, ProfitEffect.None),
        ("سحب مالك", VoucherReasonScope.Payment, ProfitEffect.None),
        ("إيرادات أخرى", VoucherReasonScope.Receipt, ProfitEffect.OtherIncome),
        ("أخرى", VoucherReasonScope.Both, ProfitEffect.None),
    ];

    internal static readonly (string Name, string IconKey)[] ExpenseCategories =
    [
        ("إيجار ومرافق", "home"),
        ("رواتب وأجور", "users"),
        ("نقل وشحن", "truck"),
        ("مصاريف إدارية", "settings"),
        ("صيانة", "wrench"),
        ("ضيافة", "coffee"),
        ("كهرباء وماء", "zap"),
        ("أخرى", "more-horizontal"),
    ];

    public const string GeneralCategoryName = "عام";
    public const string MainWarehouseName = "المخزن الرئيسي";
    public const string WalkInCustomerName = "زبون نقدي";
    public const string MainCashAccountName = "الصندوق الرئيسي";
    public const string MainBankAccountName = "الحساب البنكي";

    /// <summary>Adds missing seed rows. Returns the number of rows inserted.</summary>
    public static async Task<int> SeedAsync(MizadDbContext context, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);

        await AddMissingAsync(context.Units, u => u.Name, UnitNames, name => new Unit { Name = name }, cancellationToken);

        await AddMissingAsync(context.Categories, c => c.Name, [GeneralCategoryName], name => new Category { Name = name }, cancellationToken);

        if (!await context.Warehouses.AnyAsync(w => w.IsDefault, cancellationToken)
            && !await context.Warehouses.AnyAsync(w => w.Name == MainWarehouseName, cancellationToken))
        {
            context.Warehouses.Add(new Warehouse { Code = "MAIN", Name = MainWarehouseName, IsDefault = true });
        }

        if (!await context.Customers.AnyAsync(c => c.IsWalkIn, cancellationToken))
        {
            context.Customers.Add(new Customer { Code = "WALKIN", Name = WalkInCustomerName, IsWalkIn = true, AllowCredit = false });
        }

        await AddDefaultCashAccountAsync(context, CashAccountType.Cash, MainCashAccountName, cancellationToken);
        await AddDefaultCashAccountAsync(context, CashAccountType.Bank, MainBankAccountName, cancellationToken);

        await AddMissingAsync(
            context.AdjustmentReasons, r => r.Name, AdjustmentReasons.Select(r => r.Name),
            name =>
            {
                var seed = AdjustmentReasons.Single(r => r.Name == name);
                return new AdjustmentReason { Name = name, Direction = seed.Direction, IsSystem = seed.IsSystem };
            },
            cancellationToken);

        await AddMissingAsync(
            context.ReturnReasons, r => r.Name, ReturnReasons.Select(r => r.Name),
            name =>
            {
                var seed = ReturnReasons.Single(r => r.Name == name);
                return new ReturnReason { Name = name, AppliesTo = seed.Scope, DefaultRestock = seed.DefaultRestock };
            },
            cancellationToken);

        await AddMissingAsync(
            context.VoucherReasons, r => r.Name, VoucherReasons.Select(r => r.Name),
            name =>
            {
                var seed = VoucherReasons.Single(r => r.Name == name);
                return new VoucherReason { Name = name, Scope = seed.Scope, AffectsProfit = seed.Effect };
            },
            cancellationToken);

        await AddMissingAsync(
            context.ExpenseCategories, c => c.Name, ExpenseCategories.Select(c => c.Name),
            name => new ExpenseCategory { Name = name, IconKey = ExpenseCategories.Single(c => c.Name == name).IconKey },
            cancellationToken);

        await AddMissingAsync(
            context.DocumentSequences, s => s.DocumentType, DocumentSequenceDefaults.All.Select(s => s.DocumentType),
            type => new DocumentSequence { DocumentType = type, Prefix = DocumentSequenceDefaults.All.Single(s => s.DocumentType == type).Prefix },
            cancellationToken);

        await AddMissingAsync(
            context.Settings, s => s.Key, SettingKeys.All.Select(s => s.Key),
            key => new Setting { Key = key, Value = SettingKeys.All.Single(s => s.Key == key).DefaultText },
            cancellationToken);

        return await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task AddDefaultCashAccountAsync(MizadDbContext context, CashAccountType type, string name, CancellationToken cancellationToken)
    {
        if (!await context.CashAccounts.AnyAsync(a => a.AccountType == type && a.IsDefault, cancellationToken)
            && !await context.CashAccounts.AnyAsync(a => a.Name == name, cancellationToken))
        {
            context.CashAccounts.Add(new CashAccount { Name = name, AccountType = type, IsDefault = true });
        }
    }

    private static async Task AddMissingAsync<TEntity>(
        DbSet<TEntity> set,
        System.Linq.Expressions.Expression<Func<TEntity, string>> keySelector,
        IEnumerable<string> wantedKeys,
        Func<string, TEntity> create,
        CancellationToken cancellationToken)
        where TEntity : class
    {
        var existing = await set.Select(keySelector).ToListAsync(cancellationToken);
        var existingSet = existing.ToHashSet(StringComparer.Ordinal);
        foreach (var key in wantedKeys.Where(k => !existingSet.Contains(k)))
        {
            set.Add(create(key));
        }
    }
}

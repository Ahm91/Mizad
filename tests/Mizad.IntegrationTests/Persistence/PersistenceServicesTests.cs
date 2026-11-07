using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Mizad.Application.Abstractions;
using Mizad.Application.Abstractions.Persistence;
using Mizad.Domain.Administration;
using Mizad.Domain.Catalog;
using Mizad.Domain.Common;
using Mizad.Domain.Treasury;

namespace Mizad.IntegrationTests.Persistence;

/// <summary>Unit of work, numbering, server clock, audit stamps/log and settings (tasks 2.10, 2.15–2.17).</summary>
public sealed class PersistenceServicesTests(SqlServerFixture fixture)
{
    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    private IUnitOfWork UnitOfWork => fixture.Database.Get<IUnitOfWork>();

    private ISequenceAllocator Sequences => fixture.Database.Get<ISequenceAllocator>();

    [Fact]
    [Trait("TS", "TS-013")]
    public async Task FiftyParallelAllocations_AreUniqueAndContiguous()
    {
        var tasks = Enumerable.Range(0, 50).Select(_ => Task.Run(
            () => UnitOfWork.ExecuteInTransactionAsync(
                (context, token) => Sequences.NextAsync(context, DocumentTypes.CashTransfer, token), CancellationToken),
            CancellationToken));

        var numbers = await Task.WhenAll(tasks);

        numbers.ShouldAllBe(n => n.StartsWith("CT-", StringComparison.Ordinal) && n.Length == "CT-000000".Length);
        numbers.Distinct().Count().ShouldBe(50);
        var values = numbers.Select(n => int.Parse(n[3..], CultureInfo.InvariantCulture)).Order().ToArray();
        (values[^1] - values[0]).ShouldBe(49, "no gaps between committed allocations (D-010)");
    }

    [Fact]
    [Trait("TS", "TS-013")]
    public async Task RolledBackAllocation_ReleasesTheNumber()
    {
        var first = await UnitOfWork.ExecuteInTransactionAsync(
            (context, token) => Sequences.NextAsync(context, DocumentTypes.StockTransfer, token), CancellationToken);

        await Should.ThrowAsync<InvalidOperationException>(() => UnitOfWork.ExecuteInTransactionAsync<string>(
            async (context, token) =>
            {
                await Sequences.NextAsync(context, DocumentTypes.StockTransfer, token);
                throw new InvalidOperationException("simulated failure after numbering");
            },
            CancellationToken));

        var next = await UnitOfWork.ExecuteInTransactionAsync(
            (context, token) => Sequences.NextAsync(context, DocumentTypes.StockTransfer, token), CancellationToken);

        int.Parse(next[3..], CultureInfo.InvariantCulture).ShouldBe(int.Parse(first[3..], CultureInfo.InvariantCulture) + 1);
    }

    [Fact]
    public async Task Allocation_OutsideTransaction_IsRefused()
    {
        await using var context = fixture.Database.CreateContext();

        await Should.ThrowAsync<InvalidOperationException>(() => Sequences.NextAsync(context, DocumentTypes.Expense, CancellationToken));
    }

    [Fact]
    public async Task ServerClock_ReturnsSqlServerTime()
    {
        var serverNow = await UnitOfWork.ReadAsync(
            (context, token) => fixture.Database.Get<IServerClock>().NowAsync(context, token), CancellationToken);

        (serverNow - DateTime.Now).Duration().ShouldBeLessThan(TimeSpan.FromMinutes(5));
    }

    [Fact]
    public async Task SaveInterceptor_StampsCreationAndModification()
    {
        await using var context = fixture.Database.CreateContext();
        var category = new Category { Name = "ختم " + Guid.NewGuid().ToString("N")[..8] };
        context.Categories.Add(category);
        await context.SaveChangesAsync(CancellationToken);

        category.CreatedAtUtc.ShouldBeInRange(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
        category.UpdatedAtUtc.ShouldBeNull();

        category.SortOrder = 5;
        await context.SaveChangesAsync(CancellationToken);

        category.UpdatedAtUtc.ShouldNotBeNull();
    }

    [Fact]
    public async Task AuditEntry_IsCommittedWithTheTransaction()
    {
        var marker = Guid.NewGuid().ToString("N");

        await UnitOfWork.ExecuteInTransactionAsync(
            (context, _) =>
            {
                fixture.Database.Get<IAuditLogger>().Log(context, new AuditEntry("Test.Action", "اختبار التدقيق", "Test", marker));
                return Task.FromResult(true);
            },
            CancellationToken);

        await using var check = fixture.Database.CreateContext();
        var entry = await check.AuditLogs.SingleAsync(a => a.EntityId == marker, CancellationToken);
        entry.MachineName.ShouldBe(Environment.MachineName);
        entry.Summary.ShouldBe("اختبار التدقيق");
    }

    [Fact]
    public async Task Settings_DefaultsRoundTripAndAudit()
    {
        var settings = fixture.Database.Get<ISettingsService>();

        (await settings.GetAsync(SettingKeys.TaxEnabled, CancellationToken)).ShouldBeFalse();
        (await settings.GetAsync(SettingKeys.SalesMaxDiscountPercent, CancellationToken)).ShouldBe(10m);
        (await settings.GetAsync(SettingKeys.BackupTime, CancellationToken)).ShouldBe(new TimeOnly(2, 0));

        (await settings.SetAsync(SettingKeys.TaxRate, 5.5m, CancellationToken)).IsSuccess.ShouldBeTrue();

        (await settings.GetAsync(SettingKeys.TaxRate, CancellationToken)).ShouldBe(5.5m);
        await using var context = fixture.Database.CreateContext();
        (await context.AuditLogs.AnyAsync(a => a.Action == "Settings.Changed" && a.EntityId == SettingKeys.TaxRate.Key, CancellationToken)).ShouldBeTrue();
    }

    [Fact]
    public async Task CurrencyDecimals_AreLockedAfterTheFirstPostedDocument()
    {
        await using var database = new TestDatabase();
        await database.EnsureReadyAsync();
        var settings = database.Get<ISettingsService>();

        (await settings.SetAsync(SettingKeys.CurrencyDecimals, 2, CancellationToken)).IsSuccess.ShouldBeTrue();

        await using (var context = database.CreateContext())
        {
            var category = await context.ExpenseCategories.FirstAsync(CancellationToken);
            var cash = await context.CashAccounts.FirstAsync(a => a.AccountType == CashAccountType.Cash, CancellationToken);
            context.Expenses.Add(new Expense
            {
                Status = DocumentStatus.Posted,
                Number = "EX-000001",
                DocumentDate = DateTime.Now,
                ExpenseCategoryId = category.Id,
                Amount = 1000m,
                PaymentMethod = PaymentMethod.Cash,
                CashAccountId = cash.Id,
                Description = "اختبار",
            });
            await context.SaveChangesAsync(CancellationToken);
        }

        var result = await settings.SetAsync(SettingKeys.CurrencyDecimals, 3, CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Settings.CurrencyDecimalsLocked");
        (await settings.GetAsync(SettingKeys.CurrencyDecimals, CancellationToken)).ShouldBe(2);
    }
}

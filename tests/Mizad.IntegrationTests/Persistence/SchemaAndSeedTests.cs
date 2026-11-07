using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Mizad.Application.Abstractions.Database;
using Mizad.Domain.Administration;
using Mizad.Domain.Treasury;
using Mizad.Infrastructure.Persistence.Seed;

namespace Mizad.IntegrationTests.Persistence;

/// <summary>TS-010: the database produced by the production initializer matches DATABASE_DESIGN.</summary>
public sealed class SchemaAndSeedTests(SqlServerFixture fixture)
{
    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    [Fact]
    [Trait("TS", "TS-010")]
    public async Task Initializer_LeavesDatabaseUpToDate()
    {
        var status = await fixture.Database.Get<IDatabaseInitializer>().GetStatusAsync(CancellationToken);

        status.State.ShouldBe(DatabaseState.UpToDate);
        status.PendingMigrations.ShouldBeEmpty();
    }

    [Fact]
    [Trait("TS", "TS-010")]
    public async Task Database_HasArabicCollation_AndReadCommittedSnapshot()
    {
        var collation = await TestDatabase.ScalarAsync(fixture.ConnectionString, "SELECT CONVERT(nvarchar(128), DATABASEPROPERTYEX(DB_NAME(), 'Collation'))");
        var rcsi = await TestDatabase.ScalarAsync(fixture.ConnectionString, "SELECT is_read_committed_snapshot_on FROM sys.databases WHERE name = DB_NAME()");

        collation.ShouldBe(SqlServerFixture.DatabaseCollation);
        Convert.ToBoolean(rcsi, CultureInfo.InvariantCulture).ShouldBeTrue();
    }

    [Fact]
    [Trait("TS", "TS-010")]
    public async Task Seed_ContainsReferenceData_AndNoUsers()
    {
        await using var context = fixture.Database.CreateContext();

        (await context.Units.Select(u => u.Name).ToListAsync(CancellationToken)).ShouldBeSubsetOf(DatabaseSeeder.UnitNames);
        (await context.Units.CountAsync(CancellationToken)).ShouldBe(DatabaseSeeder.UnitNames.Length);
        (await context.Warehouses.CountAsync(w => w.IsDefault, CancellationToken)).ShouldBe(1);
        (await context.Customers.SingleAsync(c => c.IsWalkIn, CancellationToken)).AllowCredit.ShouldBeFalse();
        (await context.CashAccounts.CountAsync(a => a.IsDefault && a.AccountType == CashAccountType.Cash, CancellationToken)).ShouldBe(1);
        (await context.CashAccounts.CountAsync(a => a.IsDefault && a.AccountType == CashAccountType.Bank, CancellationToken)).ShouldBe(1);
        (await context.AdjustmentReasons.CountAsync(r => r.IsSystem, CancellationToken)).ShouldBe(1);
        (await context.ReturnReasons.CountAsync(CancellationToken)).ShouldBe(DatabaseSeeder.ReturnReasons.Length);
        (await context.VoucherReasons.CountAsync(CancellationToken)).ShouldBe(DatabaseSeeder.VoucherReasons.Length);
        (await context.ExpenseCategories.CountAsync(CancellationToken)).ShouldBe(DatabaseSeeder.ExpenseCategories.Length);
        (await context.DocumentSequences.CountAsync(CancellationToken)).ShouldBe(DocumentSequenceDefaults.All.Count);
        (await context.Settings.CountAsync(CancellationToken)).ShouldBe(SettingKeys.All.Count);
        (await context.Users.CountAsync(CancellationToken)).ShouldBe(0, "no default users — the first admin is created by the setup wizard (D-017)");
    }

    [Fact]
    [Trait("TS", "TS-010")]
    public async Task SeedSync_IsIdempotent()
    {
        await using var context = fixture.Database.CreateContext();
        var settingsBefore = await context.Settings.CountAsync(CancellationToken);

        var inserted = await DatabaseSeeder.SeedAsync(context, CancellationToken);

        inserted.ShouldBe(0);
        (await context.Settings.CountAsync(CancellationToken)).ShouldBe(settingsBefore);
    }

    [Fact]
    public async Task SearchName_IsNormalizedOnSave()
    {
        await using var context = fixture.Database.CreateContext();
        var product = await TestData.AddProductAsync(context);
        product.Name = "مُحَمَّد أحمد مكتبة";
        await context.SaveChangesAsync(CancellationToken);

        var stored = await TestDatabase.ScalarAsync(fixture.ConnectionString, "SELECT SearchName FROM dbo.Products WHERE Id = @id", ("@id", product.Id));

        stored.ShouldBe("محمد احمد مكتبه");
    }
}

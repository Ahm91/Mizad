using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Mizad.Application.Abstractions.Database;

namespace Mizad.IntegrationTests.Persistence;

/// <summary>Automatic, safe creation and upgrade of the database (D-043, TS-016…TS-020).</summary>
public sealed class DatabaseInitializerTests
{
    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    [Fact]
    [Trait("TS", "TS-016")]
    public async Task MissingDatabase_IsCreatedMigratedAndSeeded_SecondRunIsNoOp()
    {
        await using var database = new TestDatabase();
        var initializer = database.Get<IDatabaseInitializer>();
        (await initializer.GetStatusAsync(CancellationToken)).State.ShouldBe(DatabaseState.Missing);
        var progress = new List<string>();

        var first = await initializer.EnsureReadyAsync(new SynchronousProgress(progress), CancellationToken);
        var second = await initializer.EnsureReadyAsync(null, CancellationToken);

        first.IsSuccess.ShouldBeTrue(first.IsFailure ? first.Error.Message : null);
        second.IsSuccess.ShouldBeTrue();
        second.Value.State.ShouldBe(DatabaseState.UpToDate);
        progress.ShouldContain("جارٍ إنشاء قاعدة البيانات…");
        (await database.CountCopyOnlyBackupsAsync()).ShouldBe(0, "a database created just now needs no pre-migration backup");
        await using var context = database.CreateContext();
        (await context.AuditLogs.CountAsync(a => a.Action == "Database.Created", CancellationToken)).ShouldBe(1);
        (await context.AuditLogs.CountAsync(a => a.Action == "Database.Migrated", CancellationToken)).ShouldBe(0);
    }

    [Fact]
    [Trait("TS", "TS-017")]
    public async Task OutdatedDatabase_IsBackedUpBeforeMigrating()
    {
        await using var database = new TestDatabase();
        await database.CreateEmptyAsync();
        var initializer = database.Get<IDatabaseInitializer>();
        (await initializer.GetStatusAsync(CancellationToken)).State.ShouldBe(DatabaseState.PendingMigrations);
        var progress = new List<string>();

        var result = await initializer.EnsureReadyAsync(new SynchronousProgress(progress), CancellationToken);

        result.IsSuccess.ShouldBeTrue(result.IsFailure ? result.Error.Message : null);
        result.Value.State.ShouldBe(DatabaseState.UpToDate);
        (await database.CountCopyOnlyBackupsAsync()).ShouldBe(1);
        progress.ShouldContain("جارٍ أخذ نسخة احتياطية قبل تحديث قاعدة البيانات…");
        progress.ShouldContain(p => p.StartsWith("جارٍ تحديث قاعدة البيانات (1 من", StringComparison.Ordinal));
        await using var context = database.CreateContext();
        (await context.AuditLogs.CountAsync(a => a.Action == "Database.Migrated", CancellationToken)).ShouldBe(1);
    }

    [Fact]
    [Trait("TS", "TS-018")]
    public async Task ThreeDevicesStartingTogether_MigrateOnceWithoutErrors()
    {
        await using var database = new TestDatabase();
        await database.CreateEmptyAsync();
        await using var device2 = new TestDatabase(connectionStringOverride: database.ConnectionString);
        await using var device3 = new TestDatabase(connectionStringOverride: database.ConnectionString);

        var results = await Task.WhenAll(
            Task.Run(() => database.Get<IDatabaseInitializer>().EnsureReadyAsync(null, CancellationToken), CancellationToken),
            Task.Run(() => device2.Get<IDatabaseInitializer>().EnsureReadyAsync(null, CancellationToken), CancellationToken),
            Task.Run(() => device3.Get<IDatabaseInitializer>().EnsureReadyAsync(null, CancellationToken), CancellationToken));

        results.ShouldAllBe(r => r.IsSuccess);
        (await database.CountCopyOnlyBackupsAsync()).ShouldBe(1, "only the device that actually migrated takes a backup");
        await using var context = database.CreateContext();
        (await context.AuditLogs.CountAsync(a => a.Action == "Database.Migrated", CancellationToken)).ShouldBe(1);
        var historyRows = Convert.ToInt32(
            await TestDatabase.ScalarAsync(database.ConnectionString, "SELECT COUNT(*) FROM dbo.__EFMigrationsHistory"),
            CultureInfo.InvariantCulture);
        historyRows.ShouldBe(context.Database.GetMigrations().Count());
    }

    [Fact]
    [Trait("TS", "TS-019")]
    public async Task FailedBackup_PreventsMigration_AndKeepsDatabaseUntouched()
    {
        await using var database = new TestDatabase(backupDirectoryOverride: @"Z:\Mizad_NoSuchBackupFolder");
        await database.CreateEmptyAsync();
        var initializer = database.Get<IDatabaseInitializer>();

        var result = await initializer.EnsureReadyAsync(null, CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Database.BackupBeforeMigrationFailed");
        result.Error.Message.ShouldContain("حفاظاً على بياناتك");
        (await initializer.GetStatusAsync(CancellationToken)).State.ShouldBe(DatabaseState.PendingMigrations);
    }

    [Fact]
    [Trait("TS", "TS-019")]
    public async Task FailingMigration_IsReportedInArabic_AfterTheBackup_AndDatabaseStaysOnLastGoodState()
    {
        await using var database = new TestDatabase();
        await database.CreateEmptyAsync();
        // A read-only database can be backed up but not migrated — a realistic mid-upgrade failure.
        await TestDatabase.ScalarAsync(TestDatabase.ServerConnectionString, $"ALTER DATABASE [{database.Name}] SET READ_ONLY;");
        var initializer = database.Get<IDatabaseInitializer>();

        var result = await initializer.EnsureReadyAsync(null, CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Database.MigrationFailed");
        result.Error.Message.ShouldContain("بقيت البيانات على آخر حالة سليمة");
        (await database.CountCopyOnlyBackupsAsync()).ShouldBe(1, "the backup is taken before any migration runs");
        (await initializer.GetStatusAsync(CancellationToken)).State.ShouldBe(DatabaseState.PendingMigrations);
    }

    [Fact]
    [Trait("TS", "TS-020")]
    public async Task DatabaseFromNewerVersion_IsDetected_AndNotTouched()
    {
        await using var database = new TestDatabase();
        await database.EnsureReadyAsync();
        await TestDatabase.ScalarAsync(
            database.ConnectionString,
            "INSERT dbo.__EFMigrationsHistory (MigrationId, ProductVersion) VALUES (N'29991231235959_FromTheFuture', N'10.0.14');");
        var initializer = database.Get<IDatabaseInitializer>();

        var status = await initializer.GetStatusAsync(CancellationToken);
        var result = await initializer.EnsureReadyAsync(null, CancellationToken);

        status.State.ShouldBe(DatabaseState.NewerThanApp);
        status.UnknownAppliedMigrations.ShouldBe(["29991231235959_FromTheFuture"]);
        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Database.NewerThanApp");
    }

    [Fact]
    public async Task UnreachableServer_IsReported_WithArabicMessage()
    {
        await using var database = new TestDatabase(
            connectionStringOverride: "Server=tcp:127.0.0.1,1;Database=Mizad_X;Integrated Security=True;TrustServerCertificate=True;Connect Timeout=3");
        var initializer = database.Get<IDatabaseInitializer>();

        (await initializer.GetStatusAsync(CancellationToken)).State.ShouldBe(DatabaseState.Unreachable);
        var result = await initializer.EnsureReadyAsync(null, CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Database.Unreachable");
    }

    /// <summary><see cref="Progress{T}"/> posts asynchronously; tests need the messages synchronously.</summary>
    private sealed class SynchronousProgress(List<string> messages) : IProgress<StartupProgress>
    {
        public void Report(StartupProgress value) => messages.Add(value.Message);
    }
}

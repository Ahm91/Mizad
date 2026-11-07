using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Logging;
using Mizad.Application.Abstractions;
using Mizad.Application.Abstractions.Database;
using Mizad.Domain.Administration;
using Mizad.Domain.Common;
using Mizad.Infrastructure.Persistence.Seed;

namespace Mizad.Infrastructure.Persistence;

/// <summary>
/// Automatic, safe database creation and upgrade (D-043, tasks 2.14 and 2.21–2.25):
/// lock ⇒ re-check ⇒ verified PreMigration backup ⇒ migrations one by one ⇒ seed sync ⇒ verify ⇒ audit.
/// Never asks the user anything; reports progress for the splash screen (D-044).
/// </summary>
internal sealed class DatabaseInitializer(
    IConnectionStringProvider connectionStringProvider,
    IDbContextFactory<MizadDbContext> contextFactory,
    IDatabaseBackupService backupService,
    IAuditLogger auditLogger,
    ILogger<DatabaseInitializer> logger) : IDatabaseInitializer
{
    public const string RequiredCollation = "Arabic_100_CI_AI";

    /// <summary>How long a device waits for another device that is upgrading the database.</summary>
    internal static readonly TimeSpan OtherDeviceWaitTimeout = TimeSpan.FromMinutes(15);

    public async Task<DatabaseStatus> GetStatusAsync(CancellationToken cancellationToken)
    {
        if (!connectionStringProvider.IsConfigured)
        {
            return new DatabaseStatus(DatabaseState.Unreachable, [], [], "لم يُضبط الاتصال بقاعدة البيانات بعد.");
        }

        var connectionString = connectionStringProvider.GetConnectionString();
        var database = new SqlConnectionStringBuilder(connectionString).InitialCatalog;

        try
        {
            await using var master = new SqlConnection(MasterConnectionString(connectionString));
            await master.OpenAsync(cancellationToken);
            await using var command = master.CreateCommand();
            command.CommandText = "SELECT CONVERT(nvarchar(128), DATABASEPROPERTYEX(@db, 'Collation')), DB_ID(@db);";
            command.Parameters.AddWithValue("@db", database);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            await reader.ReadAsync(cancellationToken);
            if (reader.IsDBNull(1))
            {
                return new DatabaseStatus(DatabaseState.Missing, [], []);
            }

            var collation = reader.IsDBNull(0) ? null : reader.GetString(0);
            if (!string.Equals(collation, RequiredCollation, StringComparison.OrdinalIgnoreCase))
            {
                logger.LogWarning("Database {Database} collation is {Collation}; expected {Expected} (D-041)", database, collation, RequiredCollation);
            }
        }
        catch (SqlException ex)
        {
            logger.LogWarning(ex, "Database server unreachable");
            return new DatabaseStatus(DatabaseState.Unreachable, [], [], ex.Message);
        }

        await using var context = contextFactory.CreateDbContext();
        List<string> applied;
        try
        {
            applied = (await context.Database.GetAppliedMigrationsAsync(cancellationToken)).ToList();
        }
        catch (SqlException ex)
        {
            // e.g. the login cannot open this database (4060) — same user experience as an unreachable server.
            logger.LogWarning(ex, "Cannot read migrations of {Database}", database);
            return new DatabaseStatus(DatabaseState.Unreachable, [], [], ex.Message);
        }

        var known = context.Database.GetMigrations().ToList();
        var unknown = applied.Except(known, StringComparer.Ordinal).ToList();
        var pending = known.Except(applied, StringComparer.Ordinal).ToList();

        var state = unknown.Count > 0 ? DatabaseState.NewerThanApp
            : pending.Count > 0 ? DatabaseState.PendingMigrations
            : DatabaseState.UpToDate;
        return new DatabaseStatus(state, pending, unknown);
    }

    public async Task<Result<DatabaseStatus>> EnsureReadyAsync(IProgress<StartupProgress>? progress, CancellationToken cancellationToken)
    {
        progress?.Report(new StartupProgress("جارٍ الاتصال بقاعدة البيانات…"));
        var status = await GetStatusAsync(cancellationToken);

        switch (status.State)
        {
            case DatabaseState.Unreachable:
                return Result.Failure<DatabaseStatus>(Error.Unexpected(
                    "Database.Unreachable",
                    "تعذر الاتصال بخادم قاعدة البيانات. تحقق من تشغيل SQL Server ومن إعدادات الاتصال والشبكة."));
            case DatabaseState.NewerThanApp:
                return Result.Failure<DatabaseStatus>(NewerThanAppError());
        }

        var createdNow = false;
        if (status.State == DatabaseState.Missing)
        {
            progress?.Report(new StartupProgress("جارٍ إنشاء قاعدة البيانات…"));
            createdNow = await CreateDatabaseAsync(cancellationToken);
        }

        var connectionString = connectionStringProvider.GetConnectionString();
        await using var migrationLock = await AcquireLockAsync(connectionString, progress, cancellationToken);
        if (migrationLock is null)
        {
            return Result.Failure<DatabaseStatus>(Error.Unexpected(
                "Database.LockTimeout",
                "جهاز آخر يحدّث قاعدة البيانات منذ مدة طويلة. انتظر قليلاً ثم أعد المحاولة."));
        }

        // Re-check under the lock: another device may already have upgraded the database.
        status = await GetStatusAsync(cancellationToken);
        if (status.State == DatabaseState.NewerThanApp)
        {
            return Result.Failure<DatabaseStatus>(NewerThanAppError());
        }

        var applied = new List<string>();
        if (status.PendingMigrations.Count > 0)
        {
            if (!createdNow)
            {
                progress?.Report(new StartupProgress("جارٍ أخذ نسخة احتياطية قبل تحديث قاعدة البيانات…"));
                var backup = await backupService.BackupAsync(BackupType.PreMigration, cancellationToken);
                if (backup.IsFailure)
                {
                    return Result.Failure<DatabaseStatus>(Error.Unexpected(
                        "Database.BackupBeforeMigrationFailed",
                        "لم يُطبّق تحديث قاعدة البيانات لأن النسخة الاحتياطية المسبقة تعذرت، حفاظاً على بياناتك. " + backup.Error.Message));
                }
            }

            var migrationResult = await ApplyMigrationsAsync(status.PendingMigrations, progress, applied, cancellationToken);
            if (migrationResult.IsFailure)
            {
                return Result.Failure<DatabaseStatus>(migrationResult.Error);
            }
        }

        progress?.Report(new StartupProgress("جارٍ تجهيز البيانات الأساسية…"));
        await using (var context = contextFactory.CreateDbContext())
        {
            var inserted = await DatabaseSeeder.SeedAsync(context, cancellationToken);
            if (applied.Count > 0 || createdNow)
            {
                auditLogger.Log(context, new AuditEntry(
                    createdNow ? "Database.Created" : "Database.Migrated",
                    createdNow ? "إنشاء قاعدة البيانات" : $"تحديث قاعدة البيانات ({applied.Count} ترحيل)",
                    "Database",
                    null,
                    JsonSerializer.Serialize(new { Migrations = applied, SeedRowsInserted = inserted })));
                await context.SaveChangesAsync(cancellationToken);
            }
        }

        var final = await GetStatusAsync(cancellationToken);
        if (final.State != DatabaseState.UpToDate)
        {
            return Result.Failure<DatabaseStatus>(Error.Unexpected("Database.NotReady", "لم تكتمل تهيئة قاعدة البيانات. أعد المحاولة."));
        }

        logger.LogInformation("Database ready (created: {Created}, migrations applied: {Count})", createdNow, applied.Count);
        return Result.Success(final);
    }

    private async Task<Result> ApplyMigrationsAsync(
        IReadOnlyList<string> pending, IProgress<StartupProgress>? progress, List<string> applied, CancellationToken cancellationToken)
    {
        await using var context = contextFactory.CreateDbContext();
        context.Database.SetCommandTimeout(TimeSpan.FromMinutes(30));
        var migrator = context.GetService<IMigrator>();

        for (var i = 0; i < pending.Count; i++)
        {
            var migration = pending[i];
            progress?.Report(new StartupProgress($"جارٍ تحديث قاعدة البيانات ({i + 1} من {pending.Count})…", i + 1, pending.Count));
            try
            {
                await migrator.MigrateAsync(migration, cancellationToken);
                applied.Add(migration);
                logger.LogInformation("Applied migration {Migration}", migration);
            }
            catch (Exception ex) when (ex is SqlException or InvalidOperationException or DbUpdateException)
            {
                logger.LogError(ex, "Migration {Migration} failed", migration);
                return Result.Failure(Error.Unexpected(
                    "Database.MigrationFailed",
                    $"تعذر تحديث قاعدة البيانات (الخطوة {i + 1} من {pending.Count}). بقيت البيانات على آخر حالة سليمة، والنسخة الاحتياطية محفوظة."));
            }
        }

        return Result.Success();
    }

    private async Task<bool> CreateDatabaseAsync(CancellationToken cancellationToken)
    {
        var connectionString = connectionStringProvider.GetConnectionString();
        var database = new SqlConnectionStringBuilder(connectionString).InitialCatalog;
        var masterConnectionString = MasterConnectionString(connectionString);

        await using var createLock = await MigrationLock.TryAcquireAsync(
            masterConnectionString, "Mizad.Create." + database, OtherDeviceWaitTimeout, cancellationToken)
            ?? throw new TimeoutException("Timed out waiting for another device creating the database.");

        await using var master = new SqlConnection(masterConnectionString);
        await master.OpenAsync(cancellationToken);
        await using var command = master.CreateCommand();
        command.CommandTimeout = 300;
        // The name is a parameter wrapped by QUOTENAME — never concatenated from user text.
        command.CommandText =
            "IF DB_ID(@db) IS NULL BEGIN " +
            $"DECLARE @create nvarchar(400) = N'CREATE DATABASE ' + QUOTENAME(@db) + N' COLLATE {RequiredCollation}'; " +
            "DECLARE @rcsi nvarchar(400) = N'ALTER DATABASE ' + QUOTENAME(@db) + N' SET READ_COMMITTED_SNAPSHOT ON'; " +
            "EXEC sp_executesql @create; EXEC sp_executesql @rcsi; " +
            "SELECT 1; END ELSE SELECT 0;";
        command.Parameters.AddWithValue("@db", database);
        var created = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken), System.Globalization.CultureInfo.InvariantCulture) == 1;
        logger.LogInformation("Database {Database} {Action}", database, created ? "created" : "already existed");

        // New database: drop pooled connections that may have cached "database does not exist".
        SqlConnection.ClearAllPools();
        return created;
    }

    private static async Task<MigrationLock?> AcquireLockAsync(string connectionString, IProgress<StartupProgress>? progress, CancellationToken cancellationToken)
    {
        var immediate = await MigrationLock.TryAcquireAsync(connectionString, MigrationLock.MigrationResource, TimeSpan.Zero, cancellationToken);
        if (immediate is not null)
        {
            return immediate;
        }

        progress?.Report(new StartupProgress("جارٍ تحديث قاعدة البيانات من جهاز آخر… يرجى الانتظار"));
        return await MigrationLock.TryAcquireAsync(connectionString, MigrationLock.MigrationResource, OtherDeviceWaitTimeout, cancellationToken);
    }

    private static string MasterConnectionString(string connectionString) =>
        new SqlConnectionStringBuilder(connectionString) { InitialCatalog = "master", Pooling = false }.ConnectionString;

    private static Error NewerThanAppError() => Error.BusinessRule(
        "Database.NewerThanApp",
        "قاعدة البيانات محدّثة بإصدار أحدث من مِيزاد. يرجى تحديث البرنامج على هذا الجهاز.");
}

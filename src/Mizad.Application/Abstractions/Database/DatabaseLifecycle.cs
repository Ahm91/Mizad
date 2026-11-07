using Mizad.Domain.Administration;
using Mizad.Domain.Common;

namespace Mizad.Application.Abstractions.Database;

public enum DatabaseState
{
    /// <summary>The server cannot be reached with the configured connection.</summary>
    Unreachable = 0,

    /// <summary>The server is reachable but the database does not exist.</summary>
    Missing = 1,

    /// <summary>The database exists and has migrations to apply (applied automatically — D-043).</summary>
    PendingMigrations = 2,

    UpToDate = 3,

    /// <summary>The database contains migrations unknown to this program: this device must be updated.</summary>
    NewerThanApp = 4,
}

public sealed record DatabaseStatus(
    DatabaseState State,
    IReadOnlyList<string> PendingMigrations,
    IReadOnlyList<string> UnknownAppliedMigrations,
    string? Detail = null);

/// <summary>Progress reported to the splash screen (D-044).</summary>
public sealed record StartupProgress(string Message, int? Step = null, int? TotalSteps = null);

/// <summary>
/// Creates or upgrades the database automatically and safely (D-043): app lock, verified pre-migration backup,
/// migrations one by one, seed sync. Never asks the user anything.
/// </summary>
public interface IDatabaseInitializer
{
    Task<DatabaseStatus> GetStatusAsync(CancellationToken cancellationToken);

    /// <summary>Brings the database to the current version. Returns a failure with an Arabic message if impossible.</summary>
    Task<Result<DatabaseStatus>> EnsureReadyAsync(IProgress<StartupProgress>? progress, CancellationToken cancellationToken);
}

public sealed record BackupResult(string FilePath, long? SizeBytes, bool Verified);

/// <summary>Basic SQL Server backup used before migrations (task 2.22); extended in Phase 16.</summary>
public interface IDatabaseBackupService
{
    Task<Result<BackupResult>> BackupAsync(BackupType backupType, CancellationToken cancellationToken);
}

/// <summary>Current SQL Server connection string built from the machine's connection settings.</summary>
public interface IConnectionStringProvider
{
    bool IsConfigured { get; }

    string GetConnectionString();

    /// <summary>Re-reads connection settings (after the setup wizard saves new ones).</summary>
    void Reload();
}

using Mizad.Domain.Common;

namespace Mizad.Domain.Administration;

/// <summary>Shared shop setting stored as an invariant string (BUSINESS_RULES §13).</summary>
public class Setting : IHasRowVersion
{
    public string Key { get; set; } = string.Empty;

    public string? Value { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }

    public int? UpdatedByUserId { get; set; }

    public byte[] RowVersion { get; private set; } = [];
}

/// <summary>Gap-free official numbering per document type, allocated at posting under UPDLOCK (D-010).</summary>
public class DocumentSequence : IHasRowVersion
{
    public string DocumentType { get; set; } = string.Empty;

    public string Prefix { get; set; } = string.Empty;

    public int NextNumber { get; set; } = 1;

    public byte Padding { get; set; } = 6;

    public byte[] RowVersion { get; private set; } = [];

    /// <summary>Formats a number such as <c>SB-000001</c>.</summary>
    public static string Format(string prefix, int number, byte padding) =>
        prefix + "-" + number.ToString("D" + padding, System.Globalization.CultureInfo.InvariantCulture);
}

public enum BackupType : byte
{
    Manual = 0,
    Scheduled = 1,
    PreRestore = 2,
    PreMigration = 3,
}

public enum BackupStatus : byte
{
    Running = 0,
    Succeeded = 1,
    Failed = 2,
    VerifyFailed = 3,
}

/// <summary>Row of <c>BackupHistory</c> (D-021). Mirrored to <c>backup-log.json</c> when the folder is reachable.</summary>
public class BackupRecord : Entity<int>
{
    public DateTime StartedAtUtc { get; set; }

    public DateTime? CompletedAtUtc { get; set; }

    public BackupType BackupType { get; set; }

    public string FilePath { get; set; } = string.Empty;

    public long? SizeBytes { get; set; }

    public BackupStatus Status { get; set; }

    public bool Verified { get; set; }

    public string? ErrorMessage { get; set; }

    public string MachineName { get; set; } = string.Empty;

    public int? UserId { get; set; }
}

namespace Mizad.Application.Abstractions;

/// <summary>
/// Local machine clock. Use for UI defaults and audit timestamps (UTC).
/// Posting timestamps must come from the SQL Server clock (IServerClock, task 2.16 / D-023).
/// </summary>
public interface IClock
{
    DateTime UtcNow { get; }

    DateTime LocalNow { get; }

    DateOnly Today { get; }
}

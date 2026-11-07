namespace Mizad.Domain.Common;

/// <summary>Base for all persisted entities. Identity is assigned by the database.</summary>
public abstract class Entity<TId>
    where TId : struct, IEquatable<TId>
{
    public TId Id { get; protected set; }

    /// <summary>True until the entity has been saved and received a database identity.</summary>
    public bool IsTransient => Id.Equals(default);
}

/// <summary>Entities with an optimistic-concurrency token (SQL Server <c>rowversion</c>).</summary>
public interface IHasRowVersion
{
    byte[] RowVersion { get; }
}

/// <summary>
/// Append-only records stamped once at creation (ledgers, movements). Stamped by the save interceptor (task 2.10).
/// </summary>
public interface IHasCreationStamp
{
    DateTime CreatedAtUtc { get; }

    int? CreatedByUserId { get; }

    void MarkCreated(DateTime utcNow, int? userId);
}

/// <summary>Entities whose creation and modification are stamped automatically (task 2.10).</summary>
public interface IAuditable : IHasCreationStamp
{
    DateTime? UpdatedAtUtc { get; }

    int? UpdatedByUserId { get; }

    void MarkUpdated(DateTime utcNow, int? userId);
}

/// <summary>Convenience base implementing <see cref="IAuditable"/>.</summary>
public abstract class AuditableEntity<TId> : Entity<TId>, IAuditable
    where TId : struct, IEquatable<TId>
{
    public DateTime CreatedAtUtc { get; private set; }

    public int? CreatedByUserId { get; private set; }

    public DateTime? UpdatedAtUtc { get; private set; }

    public int? UpdatedByUserId { get; private set; }

    public void MarkCreated(DateTime utcNow, int? userId)
    {
        CreatedAtUtc = utcNow;
        CreatedByUserId = userId;
    }

    public void MarkUpdated(DateTime utcNow, int? userId)
    {
        UpdatedAtUtc = utcNow;
        UpdatedByUserId = userId;
    }
}

/// <summary>Base for append-only records (movements, ledger entries, cash transactions).</summary>
public abstract class LedgerRecord : Entity<long>, IHasCreationStamp
{
    public DateTime CreatedAtUtc { get; private set; }

    public int? CreatedByUserId { get; private set; }

    public void MarkCreated(DateTime utcNow, int? userId)
    {
        CreatedAtUtc = utcNow;
        CreatedByUserId = userId;
    }
}

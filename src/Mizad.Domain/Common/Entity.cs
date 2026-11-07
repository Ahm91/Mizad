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

/// <summary>Entities whose creation/modification is stamped automatically (task 2.10 interceptor).</summary>
public interface IAuditable
{
    DateTime CreatedAtUtc { get; }

    int? CreatedByUserId { get; }

    DateTime? UpdatedAtUtc { get; }

    int? UpdatedByUserId { get; }

    void MarkCreated(DateTime utcNow, int? userId);

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

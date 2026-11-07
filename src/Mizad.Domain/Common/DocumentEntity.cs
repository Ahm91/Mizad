namespace Mizad.Domain.Common;

/// <summary>Lifecycle of every financial/stock document (D-009).</summary>
public enum DocumentStatus : byte
{
    /// <summary>No effect on stock, ledgers or cash. Editable and deletable.</summary>
    Draft = 0,

    /// <summary>Immutable; effects applied atomically. Corrected only by return or void.</summary>
    Posted = 1,

    /// <summary>Reversed by compensating movements/entries; the original is kept.</summary>
    Voided = 2,
}

/// <summary>
/// Standard document columns (DATABASE_DESIGN §1 «أعمدة المستند القياسية»): official number assigned at
/// posting (D-010), idempotency key (D-011), posting/void stamps and an optimistic-concurrency token.
/// </summary>
public abstract class DocumentEntity : AuditableEntity<int>, IHasRowVersion
{
    /// <summary>Official number such as <c>SB-000001</c>; null while the document is a draft.</summary>
    public string? Number { get; set; }

    public DocumentStatus Status { get; set; } = DocumentStatus.Draft;

    /// <summary>Business date/time entered by the user (local time, D-023).</summary>
    public DateTime DocumentDate { get; set; }

    public string? Notes { get; set; }

    /// <summary>Generated when the document is opened; unique, so a repeated post cannot create a duplicate.</summary>
    public Guid ClientRequestId { get; set; } = Guid.NewGuid();

    public DateTime? PostedAtUtc { get; set; }

    public int? PostedByUserId { get; set; }

    public DateTime? VoidedAtUtc { get; set; }

    public int? VoidedByUserId { get; set; }

    public string? VoidReason { get; set; }

    public byte[] RowVersion { get; private set; } = [];
}

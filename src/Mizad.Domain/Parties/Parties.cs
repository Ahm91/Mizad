using Mizad.Domain.Calculations;
using Mizad.Domain.Common;

namespace Mizad.Domain.Parties;

/// <summary>Customer (DATABASE_DESIGN §6). The single walk-in customer has <see cref="IsWalkIn"/> (BR-022).</summary>
public class Customer : AuditableEntity<int>, IHasRowVersion
{
    private string _name = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string Name
    {
        get => _name;
        set
        {
            _name = value ?? string.Empty;
            SearchName = ArabicNormalizer.Normalize(_name);
        }
    }

    public string SearchName { get; private set; } = string.Empty;

    public string? Phone { get; set; }

    public string? Phone2 { get; set; }

    public string? Address { get; set; }

    public string? TaxNumber { get; set; }

    public PriceTier PriceTier { get; set; } = PriceTier.Retail;

    public bool AllowCredit { get; set; }

    /// <summary>Null = no limit when credit is allowed (D-030).</summary>
    public decimal? CreditLimit { get; set; }

    public int? PaymentTermDays { get; set; }

    public bool IsWalkIn { get; set; }

    public bool IsActive { get; set; } = true;

    public string? Notes { get; set; }

    public byte[] RowVersion { get; private set; } = [];
}

public class Supplier : AuditableEntity<int>, IHasRowVersion
{
    private string _name = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string Name
    {
        get => _name;
        set
        {
            _name = value ?? string.Empty;
            SearchName = ArabicNormalizer.Normalize(_name);
        }
    }

    public string SearchName { get; private set; } = string.Empty;

    public string? ContactPerson { get; set; }

    public string? Phone { get; set; }

    public string? Phone2 { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public string? TaxNumber { get; set; }

    public string? Description { get; set; }

    public int? PaymentTermDays { get; set; }

    public bool IsActive { get; set; } = true;

    public string? Notes { get; set; }

    public byte[] RowVersion { get; private set; } = [];
}

/// <summary>Documented opening balance of a customer or supplier <c>OB-</c> (BR-056).</summary>
public class OpeningBalance : DocumentEntity
{
    public PartyType PartyType { get; set; }

    public int? CustomerId { get; set; }

    public int? SupplierId { get; set; }

    public BalanceSide Side { get; set; }

    public decimal Amount { get; set; }

    public DateOnly? DueDate { get; set; }
}

/// <summary>DATABASE_DESIGN §10.3.</summary>
public enum LedgerEntryType : byte
{
    OpeningBalance = 1,
    SalesInvoice = 2,
    SalesReturn = 3,
    Receipt = 4,
    Payment = 5,
    PurchaseInvoice = 6,
    PurchaseReturn = 7,
    Reversal = 8,
}

/// <summary>
/// Append-only party ledger entry — the source of truth for balances (D-007). Exactly one of Debit/Credit is
/// positive. <see cref="SettledAmount"/> is a cache of active settlements maintained in the same transaction.
/// </summary>
public class PartyLedgerEntry : LedgerRecord
{
    public PartyType PartyType { get; set; }

    public int? CustomerId { get; set; }

    public int? SupplierId { get; set; }

    public DateTime EntryDate { get; set; }

    public DateOnly? DueDate { get; set; }

    public LedgerEntryType EntryType { get; set; }

    public decimal Debit { get; set; }

    public decimal Credit { get; set; }

    public decimal SettledAmount { get; set; }

    public string SourceDocType { get; set; } = string.Empty;

    public int SourceDocId { get; set; }

    public string DocumentNumber { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public long? ReversalOfEntryId { get; set; }

    public bool IsReversed { get; set; }
}

/// <summary>Links a credit entry to a debit entry for open-item matching (explicit or FIFO — BR-052).</summary>
public class PartySettlement : Entity<long>
{
    public long DebitEntryId { get; set; }

    public long CreditEntryId { get; set; }

    public decimal Amount { get; set; }

    public bool IsAuto { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? ReversedAtUtc { get; set; }
}

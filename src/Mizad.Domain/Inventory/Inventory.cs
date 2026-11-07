using Mizad.Domain.Common;

namespace Mizad.Domain.Inventory;

public class Warehouse : AuditableEntity<int>
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public bool IsDefault { get; set; }

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Materialized on-hand quantity per (product, warehouse). Written ONLY by the inventory engine in the same
/// transaction as the movement (D-006); outbound uses an atomic conditional update (BR-012).
/// </summary>
public class StockBalance : IHasRowVersion
{
    public int ProductId { get; set; }

    public int WarehouseId { get; set; }

    public decimal Quantity { get; set; }

    public byte[] RowVersion { get; private set; } = [];
}

/// <summary>DATABASE_DESIGN §10.2.</summary>
public enum MovementType : byte
{
    OpeningBalance = 1,
    Purchase = 2,
    PurchaseReturn = 3,
    Sale = 4,
    SaleReturn = 5,
    AdjustmentIn = 6,
    AdjustmentOut = 7,
    TransferIn = 8,
    TransferOut = 9,
    VoidReversal = 10,
}

/// <summary>Append-only stock movement — the source of truth for stock (D-006). Quantity is signed, in base units.</summary>
public class StockMovement : LedgerRecord
{
    public int ProductId { get; set; }

    public int WarehouseId { get; set; }

    public MovementType MovementType { get; set; }

    public DateTime OccurredAt { get; set; }

    public decimal Quantity { get; set; }

    public decimal UnitCost { get; set; }

    public decimal TotalCost { get; set; }

    public decimal BalanceAfter { get; set; }

    public decimal TotalOnHandAfter { get; set; }

    public decimal AverageCostAfter { get; set; }

    public string SourceDocType { get; set; } = string.Empty;

    public int SourceDocId { get; set; }

    public int? SourceLineId { get; set; }

    public string DocumentNumber { get; set; } = string.Empty;

    public string? Notes { get; set; }
}

public enum AdjustmentDirection : byte
{
    In = 1,
    Out = 2,
    Both = 3,
}

public class AdjustmentReason : Entity<int>
{
    public string Name { get; set; } = string.Empty;

    public AdjustmentDirection Direction { get; set; } = AdjustmentDirection.Both;

    /// <summary>System reasons (e.g. «رصيد افتتاحي») cannot be edited or disabled.</summary>
    public bool IsSystem { get; set; }

    public bool IsActive { get; set; } = true;
}

/// <summary>Stock adjustment document <c>ADJ-</c> (BR-070); also used for opening stock (BR-015).</summary>
public class StockAdjustment : DocumentEntity
{
    public int WarehouseId { get; set; }

    public int ReasonId { get; set; }

    public int? InventoryCountId { get; set; }

    public ICollection<StockAdjustmentLine> Lines { get; } = [];
}

public class StockAdjustmentLine : Entity<int>
{
    public int StockAdjustmentId { get; set; }

    public int LineNo { get; set; }

    public int ProductId { get; set; }

    public decimal SystemQty { get; set; }

    public decimal QuantityDelta { get; set; }

    public decimal UnitCost { get; set; }

    public decimal LineValue { get; set; }
}

/// <summary>
/// Stocktake <c>CNT-</c> (BR-071). Lifecycle maps to <see cref="DocumentStatus"/>: Draft = open/counting,
/// Posted = approved, Voided = cancelled; <see cref="IsSubmitted"/> marks "counting finished, awaiting approval".
/// </summary>
public class InventoryCount : DocumentEntity
{
    public int WarehouseId { get; set; }

    public int? CategoryId { get; set; }

    public DateTime SnapshotAtUtc { get; set; }

    public bool IsSubmitted { get; set; }

    public bool TreatUncountedAsZero { get; set; }

    public int? StockAdjustmentId { get; set; }

    public ICollection<InventoryCountLine> Lines { get; } = [];
}

public class InventoryCountLine : Entity<int>
{
    public int InventoryCountId { get; set; }

    public int ProductId { get; set; }

    public decimal SnapshotQty { get; set; }

    public decimal? CountedQty { get; set; }

    public decimal? UnitCost { get; set; }

    public string? Notes { get; set; }
}

/// <summary>Transfer between warehouses <c>TR-</c> (BR-072).</summary>
public class StockTransfer : DocumentEntity
{
    public int FromWarehouseId { get; set; }

    public int ToWarehouseId { get; set; }

    public ICollection<StockTransferLine> Lines { get; } = [];
}

public class StockTransferLine : Entity<int>
{
    public int StockTransferId { get; set; }

    public int LineNo { get; set; }

    public int ProductId { get; set; }

    public int ProductUnitId { get; set; }

    public decimal Factor { get; set; }

    public decimal Quantity { get; set; }

    public decimal BaseQuantity { get; set; }
}

using Mizad.Domain.Common;

namespace Mizad.Domain.Returns;

public enum ReturnReasonScope : byte
{
    Sales = 1,
    Purchase = 2,
    Both = 3,
}

public class ReturnReason : Entity<int>
{
    public string Name { get; set; } = string.Empty;

    public ReturnReasonScope AppliesTo { get; set; } = ReturnReasonScope.Both;

    /// <summary>Default of «إعادة إلى المخزون» when this reason is chosen (BR-046).</summary>
    public bool DefaultRestock { get; set; } = true;

    public bool IsActive { get; set; } = true;
}

/// <summary>Sales: refund cash or keep as customer credit. Purchase: receive cash or keep as supplier credit (D-015).</summary>
public enum ReturnSettlementMethod : byte
{
    CashRefund = 1,
    PartyCredit = 2,
}

/// <summary>Sales return <c>SR-</c> linked to a posted sales invoice (BR-040…BR-047).</summary>
public class SalesReturn : DocumentEntity
{
    public int SalesInvoiceId { get; set; }

    public int CustomerId { get; set; }

    public int WarehouseId { get; set; }

    public int ReturnReasonId { get; set; }

    public bool Restock { get; set; } = true;

    public ReturnSettlementMethod SettlementMethod { get; set; }

    public decimal Total { get; set; }

    public decimal CostTotal { get; set; }

    public decimal AppliedToInvoice { get; set; }

    public decimal RefundAmount { get; set; }

    public int? RefundCashAccountId { get; set; }

    public int? RefundVoucherId { get; set; }

    public int? TargetSalesInvoiceId { get; set; }

    public ICollection<SalesReturnLine> Lines { get; } = [];
}

public class SalesReturnLine : Entity<int>
{
    public int SalesReturnId { get; set; }

    public int SalesInvoiceLineId { get; set; }

    public int ProductId { get; set; }

    public int ProductUnitId { get; set; }

    public decimal Factor { get; set; }

    public decimal Quantity { get; set; }

    public decimal BaseQuantity { get; set; }

    /// <summary>Net price per base unit taken from the original line (BR-042).</summary>
    public decimal UnitNetPrice { get; set; }

    public decimal LineTotal { get; set; }

    /// <summary>Cost snapshot of the original sale line (BR-043).</summary>
    public decimal UnitCost { get; set; }

    public decimal CostTotal { get; set; }
}

/// <summary>Purchase return <c>PR-</c> linked to a posted purchase invoice (BR-045).</summary>
public class PurchaseReturn : DocumentEntity
{
    public int PurchaseInvoiceId { get; set; }

    public int SupplierId { get; set; }

    public int WarehouseId { get; set; }

    public int ReturnReasonId { get; set; }

    public ReturnSettlementMethod SettlementMethod { get; set; }

    public decimal Total { get; set; }

    public decimal AppliedToInvoice { get; set; }

    public decimal RefundAmount { get; set; }

    public int? RefundCashAccountId { get; set; }

    public int? RefundVoucherId { get; set; }

    public ICollection<PurchaseReturnLine> Lines { get; } = [];
}

public class PurchaseReturnLine : Entity<int>
{
    public int PurchaseReturnId { get; set; }

    public int PurchaseInvoiceLineId { get; set; }

    public int ProductId { get; set; }

    public int ProductUnitId { get; set; }

    public decimal Factor { get; set; }

    public decimal Quantity { get; set; }

    public decimal BaseQuantity { get; set; }

    /// <summary>Landed cost per base unit of the original purchase line.</summary>
    public decimal UnitCost { get; set; }

    public decimal LineTotal { get; set; }
}

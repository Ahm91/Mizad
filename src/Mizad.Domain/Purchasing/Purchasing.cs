using Mizad.Domain.Common;

namespace Mizad.Domain.Purchasing;

/// <summary>Purchase invoice <c>PB-</c> (BR-030…BR-034).</summary>
public class PurchaseInvoice : DocumentEntity
{
    public int SupplierId { get; set; }

    /// <summary>The supplier's own invoice number; unique per supplier among non-voided invoices (BR-032).</summary>
    public string? SupplierInvoiceNumber { get; set; }

    public int WarehouseId { get; set; }

    public DateOnly? DueDate { get; set; }

    public decimal SubTotal { get; set; }

    public decimal LinesDiscountTotal { get; set; }

    public DiscountType InvoiceDiscountType { get; set; }

    public decimal InvoiceDiscountValue { get; set; }

    public decimal InvoiceDiscountAmount { get; set; }

    public decimal Total { get; set; }

    public decimal PaidAtPurchase { get; set; }

    public PaymentMethod? PaymentMethod { get; set; }

    public int? CashAccountId { get; set; }

    public ICollection<PurchaseInvoiceLine> Lines { get; } = [];
}

public class PurchaseInvoiceLine : Entity<int>
{
    public int PurchaseInvoiceId { get; set; }

    public int LineNo { get; set; }

    public int ProductId { get; set; }

    public int ProductUnitId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string UnitName { get; set; } = string.Empty;

    public decimal Factor { get; set; }

    public decimal Quantity { get; set; }

    public decimal BaseQuantity { get; set; }

    /// <summary>Cost per entered unit (Money).</summary>
    public decimal UnitCost { get; set; }

    public decimal DiscountPercent { get; set; }

    public decimal LineDiscount { get; set; }

    public decimal LineNet { get; set; }

    public decimal AllocatedInvoiceDiscount { get; set; }

    public decimal LineTotal { get; set; }

    /// <summary>LineTotal / BaseQuantity — the cost that enters the moving average (BR-031).</summary>
    public decimal LandedUnitCost { get; set; }

    public decimal ReturnedBaseQuantity { get; set; }

    public decimal ReturnedAmount { get; set; }
}

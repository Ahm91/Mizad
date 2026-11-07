using Mizad.Domain.Common;

namespace Mizad.Domain.Sales;

/// <summary>Sales invoice <c>SB-</c> (BR-020…BR-029). Totals follow BR-091 and are recomputed on the server.</summary>
public class SalesInvoice : DocumentEntity
{
    public int CustomerId { get; set; }

    public int WarehouseId { get; set; }

    public PriceTier PriceTier { get; set; }

    public DateOnly? DueDate { get; set; }

    public decimal SubTotal { get; set; }

    public decimal LinesDiscountTotal { get; set; }

    public DiscountType InvoiceDiscountType { get; set; }

    /// <summary>The value the user typed: an amount or a percentage, depending on <see cref="InvoiceDiscountType"/>.</summary>
    public decimal InvoiceDiscountValue { get; set; }

    public decimal InvoiceDiscountAmount { get; set; }

    public decimal TaxRate { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal Total { get; set; }

    public decimal PaidAtSale { get; set; }

    public PaymentMethod? PaymentMethod { get; set; }

    public int? CashAccountId { get; set; }

    public decimal? CashTendered { get; set; }

    public decimal? ChangeGiven { get; set; }

    public decimal CostTotal { get; set; }

    public int? ShiftId { get; set; }

    public ICollection<SalesInvoiceLine> Lines { get; } = [];
}

public class SalesInvoiceLine : Entity<int>
{
    public int SalesInvoiceId { get; set; }

    public int LineNo { get; set; }

    public int ProductId { get; set; }

    public int ProductUnitId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string UnitName { get; set; } = string.Empty;

    public decimal Factor { get; set; }

    public decimal Quantity { get; set; }

    public decimal BaseQuantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal LineSubTotal { get; set; }

    public decimal LineDiscount { get; set; }

    public decimal LineNet { get; set; }

    public decimal AllocatedInvoiceDiscount { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal LineTotal { get; set; }

    /// <summary>Average cost per base unit at posting — never changes afterwards (BR-011).</summary>
    public decimal UnitCost { get; set; }

    public decimal CostTotal { get; set; }

    /// <summary>Cache of posted returns; increased atomically with a guard (BR-041).</summary>
    public decimal ReturnedBaseQuantity { get; set; }

    public decimal ReturnedAmount { get; set; }
}

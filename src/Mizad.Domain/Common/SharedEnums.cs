namespace Mizad.Domain.Common;

/// <summary>Which kind of party a ledger entry, voucher or opening balance belongs to.</summary>
public enum PartyType : byte
{
    None = 0,
    Customer = 1,
    Supplier = 2,
}

/// <summary>How money moved (D-008). "Credit/آجل" is not a method — it is the unpaid part.</summary>
public enum PaymentMethod : byte
{
    Cash = 1,
    Card = 2,
    BankTransfer = 3,
}

public enum DiscountType : byte
{
    None = 0,
    Amount = 1,
    Percent = 2,
}

/// <summary>Price list used for a customer/invoice (D-028).</summary>
public enum PriceTier : byte
{
    Retail = 0,
    Wholesale = 1,
}

/// <summary>Debit or credit side of a party opening balance.</summary>
public enum BalanceSide : byte
{
    Debit = 1,
    Credit = 2,
}

/// <summary>
/// Stable identifiers of source documents stored in ledgers and movements (DATABASE_DESIGN §10.1).
/// Also used as keys of <c>DocumentSequences</c>.
/// </summary>
public static class DocumentTypes
{
    public const string SalesInvoice = "SalesInvoice";
    public const string PurchaseInvoice = "PurchaseInvoice";
    public const string SalesReturn = "SalesReturn";
    public const string PurchaseReturn = "PurchaseReturn";
    public const string ReceiptVoucher = "ReceiptVoucher";
    public const string PaymentVoucher = "PaymentVoucher";
    public const string CashVoucher = "CashVoucher";
    public const string Expense = "Expense";
    public const string StockAdjustment = "StockAdjustment";
    public const string InventoryCount = "InventoryCount";
    public const string StockTransfer = "StockTransfer";
    public const string CashTransfer = "CashTransfer";
    public const string OpeningBalance = "OpeningBalance";
    public const string CashierShift = "CashierShift";
}

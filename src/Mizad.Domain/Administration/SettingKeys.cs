using System.Globalization;

namespace Mizad.Domain.Administration;

/// <summary>A shared setting key with its default value as an invariant string.</summary>
public abstract record SettingDefinition(string Key, string? DefaultText);

/// <summary>Typed setting definition. Values are stored with the invariant culture.</summary>
public sealed record SettingDefinition<T>(string Key, T Default) : SettingDefinition(Key, SettingValue.Format(Default));

/// <summary>Converts setting values to/from their invariant string form.</summary>
public static class SettingValue
{
    public static string? Format<T>(T value) => value switch
    {
        null => null,
        bool b => b ? "true" : "false",
        IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString(),
    };

    public static T Parse<T>(string? text, T fallback)
    {
        if (text is null)
        {
            return fallback;
        }

        var type = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
        try
        {
            object? parsed = type switch
            {
                _ when type == typeof(string) => text,
                _ when type == typeof(bool) => bool.Parse(text),
                _ when type == typeof(int) => int.Parse(text, CultureInfo.InvariantCulture),
                _ when type == typeof(decimal) => decimal.Parse(text, CultureInfo.InvariantCulture),
                _ when type == typeof(TimeOnly) => TimeOnly.Parse(text, CultureInfo.InvariantCulture),
                _ when type.IsEnum => Enum.Parse(type, text, ignoreCase: true),
                _ => throw new NotSupportedException($"Setting type {type.Name} is not supported."),
            };
            return (T)parsed!;
        }
        catch (FormatException)
        {
            return fallback;
        }
        catch (ArgumentException)
        {
            return fallback;
        }
    }
}

public enum BelowCostPolicy
{
    Block = 0,
    Warn = 1,
    Allow = 2,
}

public enum BackupFrequency
{
    Daily = 0,
    Weekly = 1,
}

/// <summary>Catalog of shared settings and their defaults (BUSINESS_RULES §13). Seeded by the seed sync (task 2.24).</summary>
public static class SettingKeys
{
    public static readonly SettingDefinition<string> ShopName = new("Shop.Name", string.Empty);
    public static readonly SettingDefinition<string> ShopAddress = new("Shop.Address", string.Empty);
    public static readonly SettingDefinition<string> ShopPhone1 = new("Shop.Phone1", string.Empty);
    public static readonly SettingDefinition<string> ShopPhone2 = new("Shop.Phone2", string.Empty);
    public static readonly SettingDefinition<string> ShopTaxNumber = new("Shop.TaxNumber", string.Empty);
    public static readonly SettingDefinition<string> ShopLogo = new("Shop.Logo", string.Empty);

    public static readonly SettingDefinition<string> CurrencyName = new("Currency.Name", "دينار عراقي");
    public static readonly SettingDefinition<string> CurrencySymbol = new("Currency.Symbol", "د.ع");
    public static readonly SettingDefinition<int> CurrencyDecimals = new("Currency.Decimals", 0);

    public static readonly SettingDefinition<bool> TaxEnabled = new("Tax.Enabled", false);
    public static readonly SettingDefinition<decimal> TaxRate = new("Tax.Rate", 0m);
    public static readonly SettingDefinition<string> TaxLabel = new("Tax.Label", "ضريبة");

    public static readonly SettingDefinition<decimal> SalesMaxDiscountPercent = new("Sales.MaxDiscountPercent", 10m);
    public static readonly SettingDefinition<BelowCostPolicy> SalesBelowCostPolicy = new("Sales.BelowCostPolicy", BelowCostPolicy.Block);
    public static readonly SettingDefinition<int> SalesDraftAlertDays = new("Sales.DraftAlertDays", 2);
    public static readonly SettingDefinition<bool> SalesAutoPrintReceipt = new("Sales.AutoPrintReceipt", false);

    public static readonly SettingDefinition<int> DocumentsMaxBackdateDays = new("Documents.MaxBackdateDays", 30);
    public static readonly SettingDefinition<int> PartiesDefaultTermDays = new("Parties.DefaultTermDays", 30);

    public static readonly SettingDefinition<bool> InventoryAllowNegativeStock = new("Inventory.AllowNegativeStock", false);
    public static readonly SettingDefinition<int> InventoryStagnantDays = new("Inventory.StagnantDays", 90);

    public static readonly SettingDefinition<bool> CashAllowNegative = new("Cash.AllowNegative", false);
    public static readonly SettingDefinition<bool> ShiftsEnabled = new("Shifts.Enabled", false);

    public static readonly SettingDefinition<int> SecurityMaxFailedLogins = new("Security.MaxFailedLogins", 5);
    public static readonly SettingDefinition<int> SecurityLockoutMinutes = new("Security.LockoutMinutes", 5);

    public static readonly SettingDefinition<DayOfWeek> ReportsWeekStartsOn = new("Reports.WeekStartsOn", DayOfWeek.Saturday);

    public static readonly SettingDefinition<bool> BackupEnabled = new("Backup.Enabled", true);
    public static readonly SettingDefinition<BackupFrequency> BackupFrequencySetting = new("Backup.Frequency", BackupFrequency.Daily);
    public static readonly SettingDefinition<TimeOnly> BackupTime = new("Backup.Time", new TimeOnly(2, 0));
    public static readonly SettingDefinition<int> BackupRetentionCount = new("Backup.RetentionCount", 14);

    /// <summary>Folder on the SQL Server machine. Empty = the instance default backup path (D-043).</summary>
    public static readonly SettingDefinition<string> BackupPath = new("Backup.Path", string.Empty);
    public static readonly SettingDefinition<string> BackupHostMachine = new("Backup.HostMachine", string.Empty);

    public static readonly SettingDefinition<int> PrintingReceiptWidthMm = new("Printing.ReceiptWidthMm", 80);
    public static readonly SettingDefinition<string> PrintingFooterText = new("Printing.FooterText", string.Empty);
    public static readonly SettingDefinition<bool> PrintingShowLogo = new("Printing.ShowLogo", true);
    public static readonly SettingDefinition<string> PrintingInvoiceSize = new("Printing.InvoiceSize", "A4");

    public static IReadOnlyList<SettingDefinition> All { get; } =
    [
        ShopName, ShopAddress, ShopPhone1, ShopPhone2, ShopTaxNumber, ShopLogo,
        CurrencyName, CurrencySymbol, CurrencyDecimals,
        TaxEnabled, TaxRate, TaxLabel,
        SalesMaxDiscountPercent, SalesBelowCostPolicy, SalesDraftAlertDays, SalesAutoPrintReceipt,
        DocumentsMaxBackdateDays, PartiesDefaultTermDays,
        InventoryAllowNegativeStock, InventoryStagnantDays,
        CashAllowNegative, ShiftsEnabled,
        SecurityMaxFailedLogins, SecurityLockoutMinutes,
        ReportsWeekStartsOn,
        BackupEnabled, BackupFrequencySetting, BackupTime, BackupRetentionCount, BackupPath, BackupHostMachine,
        PrintingReceiptWidthMm, PrintingFooterText, PrintingShowLogo, PrintingInvoiceSize,
    ];
}

/// <summary>Document numbering prefixes (D-010).</summary>
public static class DocumentSequenceDefaults
{
    public static IReadOnlyList<(string DocumentType, string Prefix)> All { get; } =
    [
        (Common.DocumentTypes.SalesInvoice, "SB"),
        (Common.DocumentTypes.PurchaseInvoice, "PB"),
        (Common.DocumentTypes.SalesReturn, "SR"),
        (Common.DocumentTypes.PurchaseReturn, "PR"),
        (Common.DocumentTypes.ReceiptVoucher, "RC"),
        (Common.DocumentTypes.PaymentVoucher, "PV"),
        (Common.DocumentTypes.Expense, "EX"),
        (Common.DocumentTypes.StockAdjustment, "ADJ"),
        (Common.DocumentTypes.InventoryCount, "CNT"),
        (Common.DocumentTypes.StockTransfer, "TR"),
        (Common.DocumentTypes.CashTransfer, "CT"),
        (Common.DocumentTypes.OpeningBalance, "OB"),
        (Common.DocumentTypes.CashierShift, "SH"),
    ];
}

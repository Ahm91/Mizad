using Mizad.Domain.Calculations;
using Mizad.Domain.Common;

namespace Mizad.Domain.Catalog;

public class Category : AuditableEntity<int>, IHasRowVersion
{
    public string Name { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public byte[] RowVersion { get; private set; } = [];
}

/// <summary>Unit of measure (حبة، كجم، كرتون…).</summary>
public class Unit : Entity<int>
{
    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Product (DATABASE_DESIGN §4). Stock and cost are always in the base unit (D-027).
/// <see cref="AverageCost"/> is written only by the inventory engine (D-006).
/// </summary>
public class Product : AuditableEntity<int>, IHasRowVersion
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

    /// <summary>Normalized name for Arabic-tolerant search (D-041).</summary>
    public string SearchName { get; private set; } = string.Empty;

    public int CategoryId { get; set; }

    public int BaseUnitId { get; set; }

    public decimal MinStockQty { get; set; }

    public decimal AverageCost { get; set; }

    public decimal? LastPurchaseCost { get; set; }

    public bool HasMultipleUnits { get; set; }

    public bool IsActive { get; set; } = true;

    public string? Notes { get; set; }

    public ICollection<ProductUnit> Units { get; } = [];

    public byte[] RowVersion { get; private set; } = [];
}

/// <summary>A unit a product is sold/bought in, with its conversion factor to the base unit and prices.</summary>
public class ProductUnit : Entity<int>
{
    public int ProductId { get; set; }

    public int UnitId { get; set; }

    /// <summary>Base units per this unit (base unit = 1).</summary>
    public decimal Factor { get; set; } = 1m;

    public bool IsBaseUnit { get; set; }

    public decimal RetailPrice { get; set; }

    public decimal? WholesalePrice { get; set; }

    public bool IsDefaultSale { get; set; }

    public bool IsDefaultPurchase { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<ProductBarcode> Barcodes { get; } = [];
}

/// <summary>A barcode of a product unit; unique across the whole system.</summary>
public class ProductBarcode : Entity<int>
{
    public int ProductUnitId { get; set; }

    public string Barcode { get; set; } = string.Empty;
}

/// <summary>Optional product picture stored in the database (D-031).</summary>
public class ProductImage
{
    public int ProductId { get; set; }

    public byte[] ImageData { get; set; } = [];

    public byte[] ThumbnailData { get; set; } = [];

    public string ContentType { get; set; } = "image/jpeg";

    public DateTime UpdatedAtUtc { get; set; }
}

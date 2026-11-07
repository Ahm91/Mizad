using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mizad.Domain.Catalog;
using Mizad.Domain.Inventory;

namespace Mizad.Infrastructure.Persistence.Configurations;

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.Property(x => x.Name).HasMaxLength(150);
        builder.HasIndex(x => x.Name).IsUnique().HasDatabaseName("UX_Categories_Name");
        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}

internal sealed class UnitConfiguration : IEntityTypeConfiguration<Unit>
{
    public void Configure(EntityTypeBuilder<Unit> builder)
    {
        builder.ToTable("Units");
        builder.Property(x => x.Name).HasMaxLength(50);
        builder.HasIndex(x => x.Name).IsUnique().HasDatabaseName("UX_Units_Name");
    }
}

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products", t =>
        {
            t.HasCheckConstraint("CK_Products_MinStockQty", "[MinStockQty] >= 0");
            t.HasCheckConstraint("CK_Products_AverageCost", "[AverageCost] >= 0");
        });
        builder.Property(x => x.Code).HasMaxLength(50);
        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UX_Products_Code");
        builder.Property(x => x.Name).HasMaxLength(200);
        builder.Property(x => x.SearchName).HasMaxLength(200);
        builder.HasIndex(x => x.SearchName);
        builder.Property(x => x.MinStockQty).AsQuantity();
        builder.Property(x => x.AverageCost).AsUnitCost();
        builder.Property(x => x.LastPurchaseCost).AsUnitCost();
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.HasOne<Category>().WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Unit>().WithMany().HasForeignKey(x => x.BaseUnitId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.CategoryId, x.IsActive });
        builder.HasMany(x => x.Units).WithOne().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}

internal sealed class ProductUnitConfiguration : IEntityTypeConfiguration<ProductUnit>
{
    public void Configure(EntityTypeBuilder<ProductUnit> builder)
    {
        builder.ToTable("ProductUnits", t =>
        {
            t.HasCheckConstraint("CK_ProductUnits_Factor", "[Factor] > 0");
            t.HasCheckConstraint("CK_ProductUnits_BaseFactor", "[IsBaseUnit] = 0 OR [Factor] = 1");
            t.HasCheckConstraint("CK_ProductUnits_RetailPrice", "[RetailPrice] >= 0");
            t.HasCheckConstraint("CK_ProductUnits_WholesalePrice", "[WholesalePrice] IS NULL OR [WholesalePrice] >= 0");
        });
        builder.Property(x => x.Factor).AsFactor();
        builder.Property(x => x.RetailPrice).AsMoney();
        builder.Property(x => x.WholesalePrice).AsMoney();
        builder.HasOne<Unit>().WithMany().HasForeignKey(x => x.UnitId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.ProductId, x.UnitId }).IsUnique().HasDatabaseName("UX_ProductUnits_ProductId_UnitId");
        builder.HasIndex(x => x.ProductId).IsUnique().HasFilter("[IsBaseUnit] = 1").HasDatabaseName("UX_ProductUnits_BaseUnit");
        builder.HasMany(x => x.Barcodes).WithOne().HasForeignKey(x => x.ProductUnitId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class ProductBarcodeConfiguration : IEntityTypeConfiguration<ProductBarcode>
{
    public void Configure(EntityTypeBuilder<ProductBarcode> builder)
    {
        builder.ToTable("ProductBarcodes");
        builder.Property(x => x.Barcode).AsCode(50);
        builder.HasIndex(x => x.Barcode).IsUnique().HasDatabaseName("UX_ProductBarcodes_Barcode");
    }
}

internal sealed class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("ProductImages");
        builder.HasKey(x => x.ProductId);
        builder.Property(x => x.ProductId).ValueGeneratedNever();
        builder.Property(x => x.ContentType).AsCode(50);
        builder.HasOne<Product>().WithOne().HasForeignKey<ProductImage>(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("Warehouses");
        builder.Property(x => x.Code).HasMaxLength(20);
        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UX_Warehouses_Code");
        builder.Property(x => x.Name).HasMaxLength(150);
        builder.HasIndex(x => x.Name).IsUnique().HasDatabaseName("UX_Warehouses_Name");
        builder.HasIndex(x => x.IsDefault).IsUnique().HasFilter("[IsDefault] = 1").HasDatabaseName("UX_Warehouses_Default");
    }
}

internal sealed class StockBalanceConfiguration : IEntityTypeConfiguration<StockBalance>
{
    public void Configure(EntityTypeBuilder<StockBalance> builder)
    {
        builder.ToTable("StockBalances");
        builder.HasKey(x => new { x.ProductId, x.WarehouseId });
        builder.Property(x => x.Quantity).AsQuantity();
        builder.HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Warehouse>().WithMany().HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}

internal sealed class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("StockMovements", t => t.HasCheckConstraint("CK_StockMovements_Quantity", "[Quantity] <> 0"));
        builder.Property(x => x.OccurredAt).HasColumnType("datetime2(3)");
        builder.Property(x => x.Quantity).AsQuantity();
        builder.Property(x => x.UnitCost).AsUnitCost();
        builder.Property(x => x.TotalCost).AsMoney();
        builder.Property(x => x.BalanceAfter).AsQuantity();
        builder.Property(x => x.TotalOnHandAfter).AsQuantity();
        builder.Property(x => x.AverageCostAfter).AsUnitCost();
        builder.Property(x => x.SourceDocType).AsCode(30);
        builder.Property(x => x.DocumentNumber).HasMaxLength(30);
        builder.Property(x => x.Notes).HasMaxLength(500);
        builder.HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Warehouse>().WithMany().HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.ProductId, x.OccurredAt, x.Id }).IncludeProperties(x => new { x.Quantity, x.BalanceAfter });
        builder.HasIndex(x => new { x.WarehouseId, x.ProductId });
        builder.HasIndex(x => new { x.SourceDocType, x.SourceDocId });
        builder.HasIndex(x => new { x.MovementType, x.OccurredAt });
    }
}

internal sealed class AdjustmentReasonConfiguration : IEntityTypeConfiguration<AdjustmentReason>
{
    public void Configure(EntityTypeBuilder<AdjustmentReason> builder)
    {
        builder.ToTable("AdjustmentReasons");
        builder.Property(x => x.Name).HasMaxLength(100);
        builder.HasIndex(x => x.Name).IsUnique().HasDatabaseName("UX_AdjustmentReasons_Name");
    }
}

internal sealed class StockAdjustmentConfiguration : IEntityTypeConfiguration<StockAdjustment>
{
    public void Configure(EntityTypeBuilder<StockAdjustment> builder)
    {
        builder.ConfigureDocument("StockAdjustments");
        builder.HasOne<Warehouse>().WithMany().HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<AdjustmentReason>().WithMany().HasForeignKey(x => x.ReasonId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<InventoryCount>().WithMany().HasForeignKey(x => x.InventoryCountId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.StockAdjustmentId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class StockAdjustmentLineConfiguration : IEntityTypeConfiguration<StockAdjustmentLine>
{
    public void Configure(EntityTypeBuilder<StockAdjustmentLine> builder)
    {
        builder.ToTable("StockAdjustmentLines", t => t.HasCheckConstraint("CK_StockAdjustmentLines_Delta", "[QuantityDelta] <> 0"));
        builder.Property(x => x.SystemQty).AsQuantity();
        builder.Property(x => x.QuantityDelta).AsQuantity();
        builder.Property(x => x.UnitCost).AsUnitCost();
        builder.Property(x => x.LineValue).AsMoney();
        builder.HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class InventoryCountConfiguration : IEntityTypeConfiguration<InventoryCount>
{
    public void Configure(EntityTypeBuilder<InventoryCount> builder)
    {
        builder.ConfigureDocument("InventoryCounts");
        builder.Property(x => x.SnapshotAtUtc).HasColumnType("datetime2(3)");
        builder.HasOne<Warehouse>().WithMany().HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Category>().WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<StockAdjustment>().WithMany().HasForeignKey(x => x.StockAdjustmentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.InventoryCountId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class InventoryCountLineConfiguration : IEntityTypeConfiguration<InventoryCountLine>
{
    public void Configure(EntityTypeBuilder<InventoryCountLine> builder)
    {
        builder.ToTable("InventoryCountLines");
        builder.Property(x => x.SnapshotQty).AsQuantity();
        builder.Property(x => x.CountedQty).AsQuantity();
        builder.Property(x => x.UnitCost).AsUnitCost();
        builder.Property(x => x.Notes).HasMaxLength(500);
        builder.HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.InventoryCountId, x.ProductId }).IsUnique().HasDatabaseName("UX_InventoryCountLines_Product");
    }
}

internal sealed class StockTransferConfiguration : IEntityTypeConfiguration<StockTransfer>
{
    public void Configure(EntityTypeBuilder<StockTransfer> builder)
    {
        builder.ConfigureDocument("StockTransfers");
        builder.ToTable("StockTransfers", t => t.HasCheckConstraint("CK_StockTransfers_Warehouses", "[FromWarehouseId] <> [ToWarehouseId]"));
        builder.HasOne<Warehouse>().WithMany().HasForeignKey(x => x.FromWarehouseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Warehouse>().WithMany().HasForeignKey(x => x.ToWarehouseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.StockTransferId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class StockTransferLineConfiguration : IEntityTypeConfiguration<StockTransferLine>
{
    public void Configure(EntityTypeBuilder<StockTransferLine> builder)
    {
        builder.ToTable("StockTransferLines", t =>
        {
            t.HasCheckConstraint("CK_StockTransferLines_Quantity", "[Quantity] > 0 AND [BaseQuantity] > 0");
            t.HasCheckConstraint("CK_StockTransferLines_Factor", "[Factor] > 0");
        });
        builder.Property(x => x.Factor).AsFactor();
        builder.Property(x => x.Quantity).AsQuantity();
        builder.Property(x => x.BaseQuantity).AsQuantity();
        builder.HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ProductUnit>().WithMany().HasForeignKey(x => x.ProductUnitId).OnDelete(DeleteBehavior.Restrict);
    }
}

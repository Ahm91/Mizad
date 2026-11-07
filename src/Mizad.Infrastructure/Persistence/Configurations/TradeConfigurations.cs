using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mizad.Domain.Catalog;
using Mizad.Domain.Inventory;
using Mizad.Domain.Parties;
using Mizad.Domain.Purchasing;
using Mizad.Domain.Returns;
using Mizad.Domain.Sales;
using Mizad.Domain.Treasury;

namespace Mizad.Infrastructure.Persistence.Configurations;

internal sealed class SalesInvoiceConfiguration : IEntityTypeConfiguration<SalesInvoice>
{
    public void Configure(EntityTypeBuilder<SalesInvoice> builder)
    {
        builder.ConfigureDocument("SalesInvoices");
        builder.ToTable("SalesInvoices", t =>
        {
            t.HasCheckConstraint("CK_SalesInvoices_Total", "[Total] >= 0");
            t.HasCheckConstraint("CK_SalesInvoices_Paid", "[PaidAtSale] >= 0 AND [PaidAtSale] <= [Total]");
        });
        builder.Property(x => x.TaxRate).AsPercent();
        builder.HasOne<Customer>().WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Warehouse>().WithMany().HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CashAccount>().WithMany().HasForeignKey(x => x.CashAccountId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CashierShift>().WithMany().HasForeignKey(x => x.ShiftId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.CustomerId, x.DocumentDate });
        builder.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.SalesInvoiceId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class SalesInvoiceLineConfiguration : IEntityTypeConfiguration<SalesInvoiceLine>
{
    public void Configure(EntityTypeBuilder<SalesInvoiceLine> builder)
    {
        builder.ToTable("SalesInvoiceLines", t =>
        {
            t.HasCheckConstraint("CK_SalesInvoiceLines_Quantity", "[Quantity] > 0 AND [BaseQuantity] > 0");
            t.HasCheckConstraint("CK_SalesInvoiceLines_Price", "[UnitPrice] >= 0");
            t.HasCheckConstraint("CK_SalesInvoiceLines_Returned", "[ReturnedBaseQuantity] >= 0 AND [ReturnedBaseQuantity] <= [BaseQuantity]");
        });
        builder.Property(x => x.ProductName).HasMaxLength(200);
        builder.Property(x => x.UnitName).HasMaxLength(50);
        builder.Property(x => x.Factor).AsFactor();
        builder.Property(x => x.Quantity).AsQuantity();
        builder.Property(x => x.BaseQuantity).AsQuantity();
        builder.Property(x => x.UnitCost).AsUnitCost();
        builder.Property(x => x.ReturnedBaseQuantity).AsQuantity();
        builder.HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ProductUnit>().WithMany().HasForeignKey(x => x.ProductUnitId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.ProductId, x.SalesInvoiceId });
    }
}

internal sealed class PurchaseInvoiceConfiguration : IEntityTypeConfiguration<PurchaseInvoice>
{
    public void Configure(EntityTypeBuilder<PurchaseInvoice> builder)
    {
        builder.ConfigureDocument("PurchaseInvoices");
        builder.ToTable("PurchaseInvoices", t =>
        {
            t.HasCheckConstraint("CK_PurchaseInvoices_Total", "[Total] >= 0");
            t.HasCheckConstraint("CK_PurchaseInvoices_Paid", "[PaidAtPurchase] >= 0 AND [PaidAtPurchase] <= [Total]");
        });
        builder.Property(x => x.SupplierInvoiceNumber).HasMaxLength(50);
        builder.HasOne<Supplier>().WithMany().HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Warehouse>().WithMany().HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CashAccount>().WithMany().HasForeignKey(x => x.CashAccountId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.SupplierId, x.SupplierInvoiceNumber })
            .IsUnique()
            .HasFilter("[SupplierInvoiceNumber] IS NOT NULL AND [Status] <> 2")
            .HasDatabaseName("UX_PurchaseInvoices_SupplierInvoiceNumber");
        builder.HasIndex(x => new { x.SupplierId, x.DocumentDate });
        builder.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.PurchaseInvoiceId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class PurchaseInvoiceLineConfiguration : IEntityTypeConfiguration<PurchaseInvoiceLine>
{
    public void Configure(EntityTypeBuilder<PurchaseInvoiceLine> builder)
    {
        builder.ToTable("PurchaseInvoiceLines", t =>
        {
            t.HasCheckConstraint("CK_PurchaseInvoiceLines_Quantity", "[Quantity] > 0 AND [BaseQuantity] > 0");
            t.HasCheckConstraint("CK_PurchaseInvoiceLines_Cost", "[UnitCost] >= 0");
            t.HasCheckConstraint("CK_PurchaseInvoiceLines_Returned", "[ReturnedBaseQuantity] >= 0 AND [ReturnedBaseQuantity] <= [BaseQuantity]");
        });
        builder.Property(x => x.ProductName).HasMaxLength(200);
        builder.Property(x => x.UnitName).HasMaxLength(50);
        builder.Property(x => x.Factor).AsFactor();
        builder.Property(x => x.Quantity).AsQuantity();
        builder.Property(x => x.BaseQuantity).AsQuantity();
        builder.Property(x => x.DiscountPercent).AsPercent();
        builder.Property(x => x.LandedUnitCost).AsUnitCost();
        builder.Property(x => x.ReturnedBaseQuantity).AsQuantity();
        builder.HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ProductUnit>().WithMany().HasForeignKey(x => x.ProductUnitId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.ProductId, x.PurchaseInvoiceId });
    }
}

internal sealed class ReturnReasonConfiguration : IEntityTypeConfiguration<ReturnReason>
{
    public void Configure(EntityTypeBuilder<ReturnReason> builder)
    {
        builder.ToTable("ReturnReasons");
        builder.Property(x => x.Name).HasMaxLength(100);
        builder.HasIndex(x => x.Name).IsUnique().HasDatabaseName("UX_ReturnReasons_Name");
    }
}

internal sealed class SalesReturnConfiguration : IEntityTypeConfiguration<SalesReturn>
{
    public void Configure(EntityTypeBuilder<SalesReturn> builder)
    {
        builder.ConfigureDocument("SalesReturns");
        builder.ToTable("SalesReturns", t => t.HasCheckConstraint("CK_SalesReturns_Total", "[Total] >= 0 AND [RefundAmount] >= 0"));
        builder.HasOne<SalesInvoice>().WithMany().HasForeignKey(x => x.SalesInvoiceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<SalesInvoice>().WithMany().HasForeignKey(x => x.TargetSalesInvoiceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Customer>().WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Warehouse>().WithMany().HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ReturnReason>().WithMany().HasForeignKey(x => x.ReturnReasonId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CashAccount>().WithMany().HasForeignKey(x => x.RefundCashAccountId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CashVoucher>().WithMany().HasForeignKey(x => x.RefundVoucherId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.SalesReturnId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class SalesReturnLineConfiguration : IEntityTypeConfiguration<SalesReturnLine>
{
    public void Configure(EntityTypeBuilder<SalesReturnLine> builder)
    {
        builder.ToTable("SalesReturnLines", t => t.HasCheckConstraint("CK_SalesReturnLines_Quantity", "[Quantity] > 0 AND [BaseQuantity] > 0"));
        builder.Property(x => x.Factor).AsFactor();
        builder.Property(x => x.Quantity).AsQuantity();
        builder.Property(x => x.BaseQuantity).AsQuantity();
        builder.Property(x => x.UnitNetPrice).AsUnitCost();
        builder.Property(x => x.UnitCost).AsUnitCost();
        builder.HasOne<SalesInvoiceLine>().WithMany().HasForeignKey(x => x.SalesInvoiceLineId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ProductUnit>().WithMany().HasForeignKey(x => x.ProductUnitId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class PurchaseReturnConfiguration : IEntityTypeConfiguration<PurchaseReturn>
{
    public void Configure(EntityTypeBuilder<PurchaseReturn> builder)
    {
        builder.ConfigureDocument("PurchaseReturns");
        builder.ToTable("PurchaseReturns", t => t.HasCheckConstraint("CK_PurchaseReturns_Total", "[Total] >= 0 AND [RefundAmount] >= 0"));
        builder.HasOne<PurchaseInvoice>().WithMany().HasForeignKey(x => x.PurchaseInvoiceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Supplier>().WithMany().HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Warehouse>().WithMany().HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ReturnReason>().WithMany().HasForeignKey(x => x.ReturnReasonId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CashAccount>().WithMany().HasForeignKey(x => x.RefundCashAccountId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CashVoucher>().WithMany().HasForeignKey(x => x.RefundVoucherId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.PurchaseReturnId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class PurchaseReturnLineConfiguration : IEntityTypeConfiguration<PurchaseReturnLine>
{
    public void Configure(EntityTypeBuilder<PurchaseReturnLine> builder)
    {
        builder.ToTable("PurchaseReturnLines", t => t.HasCheckConstraint("CK_PurchaseReturnLines_Quantity", "[Quantity] > 0 AND [BaseQuantity] > 0"));
        builder.Property(x => x.Factor).AsFactor();
        builder.Property(x => x.Quantity).AsQuantity();
        builder.Property(x => x.BaseQuantity).AsQuantity();
        builder.Property(x => x.UnitCost).AsUnitCost();
        builder.HasOne<PurchaseInvoiceLine>().WithMany().HasForeignKey(x => x.PurchaseInvoiceLineId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ProductUnit>().WithMany().HasForeignKey(x => x.ProductUnitId).OnDelete(DeleteBehavior.Restrict);
    }
}

using Microsoft.EntityFrameworkCore;
using Mizad.Application.Abstractions.Persistence;
using Mizad.Domain.Administration;
using Mizad.Domain.Catalog;
using Mizad.Domain.Common;
using Mizad.Domain.Identity;
using Mizad.Domain.Inventory;
using Mizad.Domain.Parties;
using Mizad.Domain.Purchasing;
using Mizad.Domain.Returns;
using Mizad.Domain.Sales;
using Mizad.Domain.Treasury;

namespace Mizad.Infrastructure.Persistence;

/// <summary>
/// EF Core model of DATABASE_DESIGN.md. Created per use case via <see cref="MizadDbContextFactory"/> (D-004).
/// Every schema change requires a migration (D-043).
/// </summary>
public sealed class MizadDbContext(DbContextOptions<MizadDbContext> options) : DbContext(options), IMizadDbContext
{
    /// <summary>Dependent types whose rows may be deleted together with their principal (draft lines, product units…).</summary>
    internal static readonly IReadOnlySet<Type> CascadeAllowed = new HashSet<Type>
    {
        typeof(RolePermissionGrant), typeof(ProductUnit), typeof(ProductBarcode), typeof(ProductImage),
        typeof(StockAdjustmentLine), typeof(InventoryCountLine), typeof(StockTransferLine),
        typeof(SalesInvoiceLine), typeof(PurchaseInvoiceLine), typeof(SalesReturnLine), typeof(PurchaseReturnLine),
    };

    public DbSet<User> Users => Set<User>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<RolePermissionGrant> RolePermissions => Set<RolePermissionGrant>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<Setting> Settings => Set<Setting>();

    public DbSet<DocumentSequence> DocumentSequences => Set<DocumentSequence>();

    public DbSet<BackupRecord> BackupHistory => Set<BackupRecord>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Unit> Units => Set<Unit>();

    public DbSet<Product> Products => Set<Product>();

    public DbSet<ProductUnit> ProductUnits => Set<ProductUnit>();

    public DbSet<ProductBarcode> ProductBarcodes => Set<ProductBarcode>();

    public DbSet<ProductImage> ProductImages => Set<ProductImage>();

    public DbSet<Warehouse> Warehouses => Set<Warehouse>();

    public DbSet<StockBalance> StockBalances => Set<StockBalance>();

    public DbSet<StockMovement> StockMovements => Set<StockMovement>();

    public DbSet<AdjustmentReason> AdjustmentReasons => Set<AdjustmentReason>();

    public DbSet<StockAdjustment> StockAdjustments => Set<StockAdjustment>();

    public DbSet<StockAdjustmentLine> StockAdjustmentLines => Set<StockAdjustmentLine>();

    public DbSet<InventoryCount> InventoryCounts => Set<InventoryCount>();

    public DbSet<InventoryCountLine> InventoryCountLines => Set<InventoryCountLine>();

    public DbSet<StockTransfer> StockTransfers => Set<StockTransfer>();

    public DbSet<StockTransferLine> StockTransferLines => Set<StockTransferLine>();

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<Supplier> Suppliers => Set<Supplier>();

    public DbSet<OpeningBalance> OpeningBalances => Set<OpeningBalance>();

    public DbSet<PartyLedgerEntry> PartyLedgerEntries => Set<PartyLedgerEntry>();

    public DbSet<PartySettlement> PartySettlements => Set<PartySettlement>();

    public DbSet<SalesInvoice> SalesInvoices => Set<SalesInvoice>();

    public DbSet<SalesInvoiceLine> SalesInvoiceLines => Set<SalesInvoiceLine>();

    public DbSet<PurchaseInvoice> PurchaseInvoices => Set<PurchaseInvoice>();

    public DbSet<PurchaseInvoiceLine> PurchaseInvoiceLines => Set<PurchaseInvoiceLine>();

    public DbSet<ReturnReason> ReturnReasons => Set<ReturnReason>();

    public DbSet<SalesReturn> SalesReturns => Set<SalesReturn>();

    public DbSet<SalesReturnLine> SalesReturnLines => Set<SalesReturnLine>();

    public DbSet<PurchaseReturn> PurchaseReturns => Set<PurchaseReturn>();

    public DbSet<PurchaseReturnLine> PurchaseReturnLines => Set<PurchaseReturnLine>();

    public DbSet<CashAccount> CashAccounts => Set<CashAccount>();

    public DbSet<CashTransaction> CashTransactions => Set<CashTransaction>();

    public DbSet<CashVoucher> CashVouchers => Set<CashVoucher>();

    public DbSet<VoucherReason> VoucherReasons => Set<VoucherReason>();

    public DbSet<ExpenseCategory> ExpenseCategories => Set<ExpenseCategory>();

    public DbSet<Expense> Expenses => Set<Expense>();

    public DbSet<CashTransfer> CashTransfers => Set<CashTransfer>();

    public DbSet<CashierShift> CashierShifts => Set<CashierShift>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // D-012: money is decimal(19,4) unless a configuration says otherwise; never float/double.
        configurationBuilder.Properties<decimal>().HavePrecision(19, 4);
        configurationBuilder.Properties<DateTime>().HaveColumnType("datetime2(3)");
        configurationBuilder.Properties<DateOnly>().HaveColumnType("date");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MizadDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes().ToList())
        {
            var clrType = entityType.ClrType;
            if (!typeof(IHasCreationStamp).IsAssignableFrom(clrType))
            {
                continue;
            }

            var entity = modelBuilder.Entity(clrType);
            entity.HasOne(typeof(User)).WithMany().HasForeignKey(nameof(IHasCreationStamp.CreatedByUserId)).OnDelete(DeleteBehavior.Restrict);
            if (typeof(IAuditable).IsAssignableFrom(clrType))
            {
                entity.HasOne(typeof(User)).WithMany().HasForeignKey(nameof(IAuditable.UpdatedByUserId)).OnDelete(DeleteBehavior.Restrict);
            }
        }

        // Safety net: nothing cascades except the explicitly allowed child rows.
        foreach (var foreignKey in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
        {
            if (!CascadeAllowed.Contains(foreignKey.DeclaringEntityType.ClrType))
            {
                foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
    }
}

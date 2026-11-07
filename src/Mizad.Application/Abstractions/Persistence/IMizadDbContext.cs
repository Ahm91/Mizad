using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Mizad.Domain.Administration;
using Mizad.Domain.Catalog;
using Mizad.Domain.Identity;
using Mizad.Domain.Inventory;
using Mizad.Domain.Parties;
using Mizad.Domain.Purchasing;
using Mizad.Domain.Returns;
using Mizad.Domain.Sales;
using Mizad.Domain.Treasury;

namespace Mizad.Application.Abstractions.Persistence;

/// <summary>
/// The EF Core unit of work seen by the Application layer (D-004). Created per use case through
/// <see cref="IUnitOfWork"/> or <c>IDbContextFactory</c>; never kept alive across screens.
/// Rule: <see cref="StockBalances"/> is written only by the inventory engine (D-006).
/// </summary>
public interface IMizadDbContext
{
    DbSet<User> Users { get; }

    DbSet<Role> Roles { get; }

    DbSet<RolePermissionGrant> RolePermissions { get; }

    DbSet<AuditLog> AuditLogs { get; }

    DbSet<Setting> Settings { get; }

    DbSet<DocumentSequence> DocumentSequences { get; }

    DbSet<BackupRecord> BackupHistory { get; }

    DbSet<Category> Categories { get; }

    DbSet<Unit> Units { get; }

    DbSet<Product> Products { get; }

    DbSet<ProductUnit> ProductUnits { get; }

    DbSet<ProductBarcode> ProductBarcodes { get; }

    DbSet<ProductImage> ProductImages { get; }

    DbSet<Warehouse> Warehouses { get; }

    DbSet<StockBalance> StockBalances { get; }

    DbSet<StockMovement> StockMovements { get; }

    DbSet<AdjustmentReason> AdjustmentReasons { get; }

    DbSet<StockAdjustment> StockAdjustments { get; }

    DbSet<StockAdjustmentLine> StockAdjustmentLines { get; }

    DbSet<InventoryCount> InventoryCounts { get; }

    DbSet<InventoryCountLine> InventoryCountLines { get; }

    DbSet<StockTransfer> StockTransfers { get; }

    DbSet<StockTransferLine> StockTransferLines { get; }

    DbSet<Customer> Customers { get; }

    DbSet<Supplier> Suppliers { get; }

    DbSet<OpeningBalance> OpeningBalances { get; }

    DbSet<PartyLedgerEntry> PartyLedgerEntries { get; }

    DbSet<PartySettlement> PartySettlements { get; }

    DbSet<SalesInvoice> SalesInvoices { get; }

    DbSet<SalesInvoiceLine> SalesInvoiceLines { get; }

    DbSet<PurchaseInvoice> PurchaseInvoices { get; }

    DbSet<PurchaseInvoiceLine> PurchaseInvoiceLines { get; }

    DbSet<ReturnReason> ReturnReasons { get; }

    DbSet<SalesReturn> SalesReturns { get; }

    DbSet<SalesReturnLine> SalesReturnLines { get; }

    DbSet<PurchaseReturn> PurchaseReturns { get; }

    DbSet<PurchaseReturnLine> PurchaseReturnLines { get; }

    DbSet<CashAccount> CashAccounts { get; }

    DbSet<CashTransaction> CashTransactions { get; }

    DbSet<CashVoucher> CashVouchers { get; }

    DbSet<VoucherReason> VoucherReasons { get; }

    DbSet<ExpenseCategory> ExpenseCategories { get; }

    DbSet<Expense> Expenses { get; }

    DbSet<CashTransfer> CashTransfers { get; }

    DbSet<CashierShift> CashierShifts { get; }

    DatabaseFacade Database { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

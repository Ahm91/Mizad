using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mizad.Domain.Identity;
using Mizad.Domain.Parties;
using Mizad.Domain.Treasury;

namespace Mizad.Infrastructure.Persistence.Configurations;

internal sealed class CashAccountConfiguration : IEntityTypeConfiguration<CashAccount>
{
    public void Configure(EntityTypeBuilder<CashAccount> builder)
    {
        builder.ToTable("CashAccounts");
        builder.Property(x => x.Name).HasMaxLength(100);
        builder.HasIndex(x => x.Name).IsUnique().HasDatabaseName("UX_CashAccounts_Name");
        builder.HasIndex(x => x.AccountType).IsUnique().HasFilter("[IsDefault] = 1").HasDatabaseName("UX_CashAccounts_DefaultPerType");
        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}

internal sealed class CashTransactionConfiguration : IEntityTypeConfiguration<CashTransaction>
{
    public void Configure(EntityTypeBuilder<CashTransaction> builder)
    {
        builder.ToTable("CashTransactions", t =>
        {
            t.HasCheckConstraint("CK_CashTransactions_OneSide", "([AmountIn] > 0 AND [AmountOut] = 0) OR ([AmountOut] > 0 AND [AmountIn] = 0)");
            t.HasCheckConstraint("CK_CashTransactions_Party", ConfigurationExtensions.PartyCheckSql(allowNone: true));
        });
        builder.Property(x => x.TxDate).AsBusinessDate();
        builder.Property(x => x.AmountIn).AsMoney();
        builder.Property(x => x.AmountOut).AsMoney();
        builder.Property(x => x.SourceDocType).AsCode(30);
        builder.Property(x => x.DocumentNumber).HasMaxLength(30);
        builder.Property(x => x.Description).HasMaxLength(300);
        builder.HasOne<CashAccount>().WithMany().HasForeignKey(x => x.CashAccountId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Customer>().WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Supplier>().WithMany().HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CashierShift>().WithMany().HasForeignKey(x => x.ShiftId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CashTransaction>().WithMany().HasForeignKey(x => x.ReversalOfId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.CashAccountId, x.TxDate, x.Id }).IncludeProperties(x => new { x.AmountIn, x.AmountOut });
        builder.HasIndex(x => new { x.SourceDocType, x.SourceDocId });
        builder.HasIndex(x => x.ShiftId);
    }
}

internal sealed class CashVoucherConfiguration : IEntityTypeConfiguration<CashVoucher>
{
    public void Configure(EntityTypeBuilder<CashVoucher> builder)
    {
        builder.ConfigureDocument("CashVouchers");
        builder.ToTable("CashVouchers", t =>
        {
            t.HasCheckConstraint("CK_CashVouchers_Amount", "[Amount] > 0");
            t.HasCheckConstraint("CK_CashVouchers_Party", ConfigurationExtensions.PartyCheckSql(allowNone: true));
        });
        builder.Property(x => x.Amount).AsMoney();
        builder.Property(x => x.Description).HasMaxLength(300);
        builder.Property(x => x.OriginDocType).AsCode(30);
        builder.HasOne<Customer>().WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Supplier>().WithMany().HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<VoucherReason>().WithMany().HasForeignKey(x => x.VoucherReasonId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CashAccount>().WithMany().HasForeignKey(x => x.CashAccountId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CashierShift>().WithMany().HasForeignKey(x => x.ShiftId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.OriginDocType, x.OriginDocId });
    }
}

internal sealed class VoucherReasonConfiguration : IEntityTypeConfiguration<VoucherReason>
{
    public void Configure(EntityTypeBuilder<VoucherReason> builder)
    {
        builder.ToTable("VoucherReasons");
        builder.Property(x => x.Name).HasMaxLength(100);
        builder.HasIndex(x => x.Name).IsUnique().HasDatabaseName("UX_VoucherReasons_Name");
    }
}

internal sealed class ExpenseCategoryConfiguration : IEntityTypeConfiguration<ExpenseCategory>
{
    public void Configure(EntityTypeBuilder<ExpenseCategory> builder)
    {
        builder.ToTable("ExpenseCategories");
        builder.Property(x => x.Name).HasMaxLength(100);
        builder.HasIndex(x => x.Name).IsUnique().HasDatabaseName("UX_ExpenseCategories_Name");
        builder.Property(x => x.IconKey).AsCode(50);
    }
}

internal sealed class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.ConfigureDocument("Expenses");
        builder.ToTable("Expenses", t => t.HasCheckConstraint("CK_Expenses_Amount", "[Amount] > 0"));
        builder.Property(x => x.Amount).AsMoney();
        builder.Property(x => x.Payee).HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(300);
        builder.HasOne<ExpenseCategory>().WithMany().HasForeignKey(x => x.ExpenseCategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CashAccount>().WithMany().HasForeignKey(x => x.CashAccountId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CashierShift>().WithMany().HasForeignKey(x => x.ShiftId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class CashTransferConfiguration : IEntityTypeConfiguration<CashTransfer>
{
    public void Configure(EntityTypeBuilder<CashTransfer> builder)
    {
        builder.ConfigureDocument("CashTransfers");
        builder.ToTable("CashTransfers", t =>
        {
            t.HasCheckConstraint("CK_CashTransfers_Amount", "[Amount] > 0");
            t.HasCheckConstraint("CK_CashTransfers_Accounts", "[FromAccountId] <> [ToAccountId]");
        });
        builder.Property(x => x.Amount).AsMoney();
        builder.HasOne<CashAccount>().WithMany().HasForeignKey(x => x.FromAccountId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CashAccount>().WithMany().HasForeignKey(x => x.ToAccountId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class CashierShiftConfiguration : IEntityTypeConfiguration<CashierShift>
{
    public void Configure(EntityTypeBuilder<CashierShift> builder)
    {
        builder.ToTable("CashierShifts");
        builder.Property(x => x.Number).HasMaxLength(30);
        builder.Property(x => x.OpenedAt).AsBusinessDate();
        builder.Property(x => x.ClosedAt).AsBusinessDate();
        builder.Property(x => x.OpeningFloat).AsMoney();
        builder.Property(x => x.ExpectedCash).AsMoney();
        builder.Property(x => x.CountedCash).AsMoney();
        builder.Property(x => x.Difference).AsMoney();
        builder.Property(x => x.Notes).HasMaxLength(500);
        builder.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<User>().WithMany().HasForeignKey(x => x.ClosedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CashAccount>().WithMany().HasForeignKey(x => x.CashAccountId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.UserId).IsUnique().HasFilter("[Status] = 0").HasDatabaseName("UX_CashierShifts_OpenPerUser");
        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}

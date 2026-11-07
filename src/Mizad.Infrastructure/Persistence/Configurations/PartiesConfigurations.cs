using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mizad.Domain.Parties;

namespace Mizad.Infrastructure.Persistence.Configurations;

internal sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers", t =>
        {
            t.HasCheckConstraint("CK_Customers_WalkInNoCredit", "[IsWalkIn] = 0 OR [AllowCredit] = 0");
            t.HasCheckConstraint("CK_Customers_CreditLimit", "[CreditLimit] IS NULL OR [CreditLimit] >= 0");
            t.HasCheckConstraint("CK_Customers_PaymentTermDays", "[PaymentTermDays] IS NULL OR [PaymentTermDays] >= 0");
        });
        builder.Property(x => x.Code).HasMaxLength(20);
        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UX_Customers_Code");
        builder.Property(x => x.Name).HasMaxLength(200);
        builder.Property(x => x.SearchName).HasMaxLength(200);
        builder.HasIndex(x => x.SearchName);
        builder.Property(x => x.Phone).HasMaxLength(30);
        builder.HasIndex(x => x.Phone);
        builder.Property(x => x.Phone2).HasMaxLength(30);
        builder.Property(x => x.Address).HasMaxLength(300);
        builder.Property(x => x.TaxNumber).HasMaxLength(50);
        builder.Property(x => x.CreditLimit).AsMoney();
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.HasIndex(x => x.IsWalkIn).IsUnique().HasFilter("[IsWalkIn] = 1").HasDatabaseName("UX_Customers_WalkIn");
        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}

internal sealed class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("Suppliers", t =>
            t.HasCheckConstraint("CK_Suppliers_PaymentTermDays", "[PaymentTermDays] IS NULL OR [PaymentTermDays] >= 0"));
        builder.Property(x => x.Code).HasMaxLength(20);
        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UX_Suppliers_Code");
        builder.Property(x => x.Name).HasMaxLength(200);
        builder.Property(x => x.SearchName).HasMaxLength(200);
        builder.HasIndex(x => x.SearchName);
        builder.Property(x => x.ContactPerson).HasMaxLength(150);
        builder.Property(x => x.Phone).HasMaxLength(30);
        builder.Property(x => x.Phone2).HasMaxLength(30);
        builder.Property(x => x.Email).HasMaxLength(150);
        builder.Property(x => x.Address).HasMaxLength(300);
        builder.Property(x => x.TaxNumber).HasMaxLength(50);
        builder.Property(x => x.Description).HasMaxLength(300);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}

internal sealed class OpeningBalanceConfiguration : IEntityTypeConfiguration<OpeningBalance>
{
    public void Configure(EntityTypeBuilder<OpeningBalance> builder)
    {
        builder.ConfigureDocument("OpeningBalances");
        builder.ToTable("OpeningBalances", t =>
        {
            t.HasCheckConstraint("CK_OpeningBalances_Party", ConfigurationExtensions.PartyCheckSql(allowNone: false));
            t.HasCheckConstraint("CK_OpeningBalances_Amount", "[Amount] > 0");
        });
        builder.Property(x => x.Amount).AsMoney();
        builder.HasOne<Customer>().WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Supplier>().WithMany().HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class PartyLedgerEntryConfiguration : IEntityTypeConfiguration<PartyLedgerEntry>
{
    public void Configure(EntityTypeBuilder<PartyLedgerEntry> builder)
    {
        builder.ToTable("PartyLedgerEntries", t =>
        {
            t.HasCheckConstraint("CK_PartyLedgerEntries_Party", ConfigurationExtensions.PartyCheckSql(allowNone: false));
            t.HasCheckConstraint("CK_PartyLedgerEntries_OneSide", "([Debit] > 0 AND [Credit] = 0) OR ([Credit] > 0 AND [Debit] = 0)");
            t.HasCheckConstraint("CK_PartyLedgerEntries_Settled", "[SettledAmount] >= 0 AND [SettledAmount] <= [Debit] + [Credit]");
        });
        builder.Property(x => x.EntryDate).AsBusinessDate();
        builder.Property(x => x.Debit).AsMoney();
        builder.Property(x => x.Credit).AsMoney();
        builder.Property(x => x.SettledAmount).AsMoney();
        builder.Property(x => x.SourceDocType).AsCode(30);
        builder.Property(x => x.DocumentNumber).HasMaxLength(30);
        builder.Property(x => x.Description).HasMaxLength(300);
        builder.HasOne<Customer>().WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Supplier>().WithMany().HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<PartyLedgerEntry>().WithMany().HasForeignKey(x => x.ReversalOfEntryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.SourceDocType, x.SourceDocId, x.EntryType }).IsUnique().HasDatabaseName("UX_PartyLedgerEntries_Source");
        builder.HasIndex(x => new { x.CustomerId, x.EntryDate, x.Id }).IncludeProperties(x => new { x.Debit, x.Credit, x.SettledAmount });
        builder.HasIndex(x => new { x.SupplierId, x.EntryDate, x.Id }).IncludeProperties(x => new { x.Debit, x.Credit, x.SettledAmount });
        builder.HasIndex(x => new { x.CustomerId, x.IsReversed, x.DueDate });
        builder.HasIndex(x => new { x.SupplierId, x.IsReversed, x.DueDate });
    }
}

internal sealed class PartySettlementConfiguration : IEntityTypeConfiguration<PartySettlement>
{
    public void Configure(EntityTypeBuilder<PartySettlement> builder)
    {
        builder.ToTable("PartySettlements", t => t.HasCheckConstraint("CK_PartySettlements_Amount", "[Amount] > 0"));
        builder.Property(x => x.Amount).AsMoney();
        builder.Property(x => x.CreatedAtUtc).HasColumnType("datetime2(3)");
        builder.HasOne<PartyLedgerEntry>().WithMany().HasForeignKey(x => x.DebitEntryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<PartyLedgerEntry>().WithMany().HasForeignKey(x => x.CreditEntryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.DebitEntryId).HasFilter("[ReversedAtUtc] IS NULL");
        builder.HasIndex(x => x.CreditEntryId).HasFilter("[ReversedAtUtc] IS NULL");
    }
}

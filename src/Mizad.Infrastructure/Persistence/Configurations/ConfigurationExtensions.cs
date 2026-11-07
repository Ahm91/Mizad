using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mizad.Domain.Common;
using Mizad.Domain.Identity;

namespace Mizad.Infrastructure.Persistence.Configurations;

/// <summary>Column-type helpers matching DATABASE_DESIGN §1 (D-012).</summary>
internal static class ConfigurationExtensions
{
    /// <summary>Money: decimal(19,4) — also the model-wide default.</summary>
    public static PropertyBuilder<T> AsMoney<T>(this PropertyBuilder<T> property) => property.HasPrecision(19, 4);

    /// <summary>Unit cost: decimal(19,6).</summary>
    public static PropertyBuilder<T> AsUnitCost<T>(this PropertyBuilder<T> property) => property.HasPrecision(19, 6);

    /// <summary>Quantity: decimal(18,3).</summary>
    public static PropertyBuilder<T> AsQuantity<T>(this PropertyBuilder<T> property) => property.HasPrecision(18, 3);

    /// <summary>Unit conversion factor: decimal(18,6).</summary>
    public static PropertyBuilder<T> AsFactor<T>(this PropertyBuilder<T> property) => property.HasPrecision(18, 6);

    /// <summary>Percentage: decimal(9,4).</summary>
    public static PropertyBuilder<T> AsPercent<T>(this PropertyBuilder<T> property) => property.HasPrecision(9, 4);

    /// <summary>Business date/time without fractional seconds.</summary>
    public static PropertyBuilder<T> AsBusinessDate<T>(this PropertyBuilder<T> property) => property.HasColumnType("datetime2(0)");

    /// <summary>ASCII code/identifier column: varchar(length).</summary>
    public static PropertyBuilder<T> AsCode<T>(this PropertyBuilder<T> property, int length) =>
        property.HasMaxLength(length).IsUnicode(false);

    /// <summary>
    /// Standard document columns (DATABASE_DESIGN §1): number assigned at posting (unique when present),
    /// unique idempotency key, posting/void stamps, rowversion and lifecycle check constraints.
    /// </summary>
    public static EntityTypeBuilder<T> ConfigureDocument<T>(this EntityTypeBuilder<T> builder, string table)
        where T : DocumentEntity
    {
        builder.ToTable(table, t =>
        {
            t.HasCheckConstraint($"CK_{table}_Status", "[Status] IN (0, 1, 2)");
            t.HasCheckConstraint($"CK_{table}_NumberWhenPosted", "[Status] = 0 OR [Number] IS NOT NULL");
        });

        builder.Property(x => x.Number).HasMaxLength(30);
        builder.HasIndex(x => x.Number).IsUnique().HasFilter("[Number] IS NOT NULL").HasDatabaseName($"UX_{table}_Number");
        builder.Property(x => x.DocumentDate).AsBusinessDate();
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.VoidReason).HasMaxLength(500);
        builder.HasIndex(x => x.ClientRequestId).IsUnique().HasDatabaseName($"UX_{table}_ClientRequestId");
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasOne<User>().WithMany().HasForeignKey(x => x.PostedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<User>().WithMany().HasForeignKey(x => x.VoidedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.Status, x.DocumentDate });
        return builder;
    }

    /// <summary>Exactly one of CustomerId/SupplierId is set, matching PartyType (DATABASE_DESIGN §6).</summary>
    public static string PartyCheckSql(bool allowNone) =>
        (allowNone ? "([PartyType] = 0 AND [CustomerId] IS NULL AND [SupplierId] IS NULL) OR " : string.Empty) +
        "([PartyType] = 1 AND [CustomerId] IS NOT NULL AND [SupplierId] IS NULL) OR " +
        "([PartyType] = 2 AND [SupplierId] IS NOT NULL AND [CustomerId] IS NULL)";
}

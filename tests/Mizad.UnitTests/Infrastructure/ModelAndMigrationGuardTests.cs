using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Mizad.Domain.Common;
using Mizad.Infrastructure.Persistence;

namespace Mizad.UnitTests.Infrastructure;

/// <summary>
/// Guards of the EF model and migrations (D-012, D-043). No database connection is needed.
/// </summary>
public sealed class ModelAndMigrationGuardTests : IDisposable
{
    /// <summary>
    /// Migrations approved by the owner to contain destructive operations (DATABASE_DESIGN §14.4).
    /// Adding an id here requires the owner's approval and a data-migration plan.
    /// </summary>
    private static readonly HashSet<string> ApprovedDestructiveMigrations = [];

    /// <summary>Columns allowed to be (n)varchar(max)/varbinary(max) by design.</summary>
    private static readonly HashSet<string> UnboundedColumns =
    [
        "AuditLog.DetailsJson", "Setting.Value", "ProductImage.ImageData", "ProductImage.ThumbnailData",
    ];

    private readonly MizadDbContext _context = new(MizadDbContextOptions.Build(
        "Server=.;Database=Mizad_ModelOnly;Integrated Security=True;TrustServerCertificate=True"));

    private IModel Model => _context.GetService<IDesignTimeModel>().Model;

    [Fact]
    [Trait("TS", "TS-015")]
    public void Model_HasNoChangesWithoutAMigration()
    {
        _context.Database.HasPendingModelChanges()
            .ShouldBeFalse("the model changed but no migration was added — run `dotnet ef migrations add` (D-043)");
    }

    [Fact]
    [Trait("TS", "TS-020")]
    public void Migrations_ContainNoUnapprovedDestructiveOperations()
    {
        var migrationsAssembly = _context.GetService<IMigrationsAssembly>();

        foreach (var (id, type) in migrationsAssembly.Migrations)
        {
            if (ApprovedDestructiveMigrations.Contains(id))
            {
                continue;
            }

            var migration = migrationsAssembly.CreateMigration(type, _context.Database.ProviderName!);
            var destructive = migration.UpOperations
                .Where(op => op is DropTableOperation or DropColumnOperation or RenameColumnOperation or RenameTableOperation)
                .Select(op => op.GetType().Name)
                .ToList();

            destructive.ShouldBeEmpty($"migration {id} drops/renames data without owner approval (DATABASE_DESIGN §14.3)");
        }
    }

    [Fact]
    public void StringColumns_HaveExplicitLengths()
    {
        var unbounded = Model.GetEntityTypes()
            .SelectMany(e => e.GetProperties().Select(p => (Entity: e, Property: p)))
            .Where(x => x.Property.ClrType == typeof(string) || x.Property.ClrType == typeof(byte[]))
            .Where(x => !x.Property.IsConcurrencyToken)
            .Where(x => x.Property.GetMaxLength() is null)
            .Select(x => $"{x.Entity.ClrType.Name}.{x.Property.Name}")
            .Where(name => !UnboundedColumns.Contains(name))
            .ToList();

        unbounded.ShouldBeEmpty();
    }

    [Fact]
    public void DecimalColumns_UseApprovedPrecisions()
    {
        var approved = new HashSet<(int?, int?)> { (19, 4), (19, 6), (18, 3), (18, 6), (9, 4) };
        var wrong = Model.GetEntityTypes()
            .SelectMany(e => e.GetProperties().Select(p => (Entity: e, Property: p)))
            .Where(x => (Nullable.GetUnderlyingType(x.Property.ClrType) ?? x.Property.ClrType) == typeof(decimal))
            .Where(x => !approved.Contains((x.Property.GetPrecision(), x.Property.GetScale())))
            .Select(x => $"{x.Entity.ClrType.Name}.{x.Property.Name}")
            .ToList();

        wrong.ShouldBeEmpty();
    }

    [Fact]
    public void NoFloatingPointColumns()
    {
        Model.GetEntityTypes()
            .SelectMany(e => e.GetProperties())
            .Where(p => (Nullable.GetUnderlyingType(p.ClrType) ?? p.ClrType) is var t && (t == typeof(double) || t == typeof(float)))
            .ShouldBeEmpty("D-012: never float/double for money or quantities");
    }

    [Fact]
    public void OnlyAllowedRelationshipsCascade()
    {
        var cascading = Model.GetEntityTypes()
            .SelectMany(e => e.GetForeignKeys())
            .Where(fk => fk.DeleteBehavior == DeleteBehavior.Cascade)
            .Select(fk => fk.DeclaringEntityType.ClrType)
            .Distinct()
            .ToList();

        cascading.ShouldAllBe(type => MizadDbContext.CascadeAllowed.Contains(type));
    }

    [Fact]
    public void EveryDocument_HasUniqueNumberAndIdempotencyKey()
    {
        var documents = Model.GetEntityTypes().Where(e => typeof(DocumentEntity).IsAssignableFrom(e.ClrType)).ToList();

        documents.Count.ShouldBe(11);
        foreach (var document in documents)
        {
            var uniqueColumns = document.GetIndexes().Where(i => i.IsUnique).Select(i => string.Join(",", i.Properties.Select(p => p.Name))).ToList();
            uniqueColumns.ShouldContain(nameof(DocumentEntity.Number), document.ClrType.Name);
            uniqueColumns.ShouldContain(nameof(DocumentEntity.ClientRequestId), document.ClrType.Name);
        }
    }

    public void Dispose() => _context.Dispose();
}

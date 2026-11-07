using Microsoft.EntityFrameworkCore;
using Mizad.Domain.Catalog;
using Mizad.Infrastructure.Persistence;
using Mizad.Infrastructure.Persistence.Seed;

namespace Mizad.IntegrationTests;

/// <summary>Builders for test rows on top of the seeded reference data.</summary>
internal static class TestData
{
    public static async Task<Product> AddProductAsync(MizadDbContext context, string? barcode = null)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var category = await context.Categories.SingleAsync(c => c.Name == DatabaseSeeder.GeneralCategoryName, cancellationToken);
        var unit = await context.Units.FirstAsync(u => u.Name == "حبة", cancellationToken);
        var suffix = Guid.NewGuid().ToString("N")[..10];

        var product = new Product { Code = "P-" + suffix, Name = "صنف اختبار " + suffix, CategoryId = category.Id, BaseUnitId = unit.Id };
        var productUnit = new ProductUnit { UnitId = unit.Id, Factor = 1m, IsBaseUnit = true, RetailPrice = 1000m, IsDefaultSale = true };
        productUnit.Barcodes.Add(new ProductBarcode { Barcode = barcode ?? "B" + suffix });
        product.Units.Add(productUnit);
        context.Products.Add(product);
        await context.SaveChangesAsync(cancellationToken);
        return product;
    }
}

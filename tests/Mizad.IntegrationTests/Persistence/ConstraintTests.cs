using Microsoft.EntityFrameworkCore;
using Mizad.Domain.Catalog;
using Mizad.Domain.Common;
using Mizad.Domain.Parties;
using Mizad.Domain.Treasury;
using Mizad.Infrastructure.Persistence;

namespace Mizad.IntegrationTests.Persistence;

/// <summary>Database constraints enforce the design even if application code is wrong (TS-011, TS-012, TS-014).</summary>
public sealed class ConstraintTests(SqlServerFixture fixture)
{
    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    [Fact]
    [Trait("TS", "TS-011")]
    public async Task DuplicateBarcode_IsRejected_WithArabicMessage()
    {
        var barcode = "DUP" + Guid.NewGuid().ToString("N")[..12];
        await using (var context = fixture.Database.CreateContext())
        {
            await TestData.AddProductAsync(context, barcode);
        }

        await using var second = fixture.Database.CreateContext();
        var exception = await Should.ThrowAsync<DbUpdateException>(() => TestData.AddProductAsync(second, barcode));

        SqlErrorTranslator.TryTranslate(exception, out var error).ShouldBeTrue();
        error.Kind.ShouldBe(ErrorKind.Conflict);
        error.Message.ShouldBe("هذا الباركود مستخدم لصنف آخر.");
    }

    [Fact]
    [Trait("TS", "TS-012")]
    public async Task ConcurrentEdit_SecondSave_IsRejected()
    {
        int productId;
        await using (var setup = fixture.Database.CreateContext())
        {
            productId = (await TestData.AddProductAsync(setup)).Id;
        }

        await using var first = fixture.Database.CreateContext();
        await using var second = fixture.Database.CreateContext();
        var firstCopy = await first.Products.SingleAsync(p => p.Id == productId, CancellationToken);
        var secondCopy = await second.Products.SingleAsync(p => p.Id == productId, CancellationToken);

        firstCopy.Name = "تعديل أول";
        await first.SaveChangesAsync(CancellationToken);
        secondCopy.Name = "تعديل ثانٍ";
        var exception = await Should.ThrowAsync<DbUpdateConcurrencyException>(() => second.SaveChangesAsync(CancellationToken));

        SqlErrorTranslator.TryTranslate(exception, out var error).ShouldBeTrue();
        error.Code.ShouldBe("Database.Concurrency");
    }

    [Fact]
    [Trait("TS", "TS-014")]
    public async Task LedgerEntry_WithBothDebitAndCredit_IsRejected()
    {
        await using var context = fixture.Database.CreateContext();
        var walkIn = await context.Customers.SingleAsync(c => c.IsWalkIn, CancellationToken);
        context.PartyLedgerEntries.Add(new PartyLedgerEntry
        {
            PartyType = PartyType.Customer,
            CustomerId = walkIn.Id,
            EntryDate = DateTime.Now,
            EntryType = LedgerEntryType.SalesInvoice,
            Debit = 5m,
            Credit = 5m,
            SourceDocType = DocumentTypes.SalesInvoice,
            SourceDocId = -1,
            DocumentNumber = "TEST",
            Description = "اختبار",
        });

        var exception = await Should.ThrowAsync<DbUpdateException>(() => context.SaveChangesAsync(CancellationToken));

        SqlErrorTranslator.TryTranslate(exception, out var error).ShouldBeTrue();
        error.Code.ShouldBe("Database.CheckViolation");
    }

    [Fact]
    [Trait("TS", "TS-014")]
    public async Task ProductUnit_WithZeroFactor_IsRejected()
    {
        await using var context = fixture.Database.CreateContext();
        var product = await TestData.AddProductAsync(context);
        var carton = await context.Units.SingleAsync(u => u.Name == "كرتون", CancellationToken);
        context.ProductUnits.Add(new ProductUnit { ProductId = product.Id, UnitId = carton.Id, Factor = 0m, RetailPrice = 1m });

        await Should.ThrowAsync<DbUpdateException>(() => context.SaveChangesAsync(CancellationToken));
    }

    [Fact]
    [Trait("TS", "TS-014")]
    public async Task PostedDocument_WithoutNumber_IsRejected()
    {
        await using var context = fixture.Database.CreateContext();
        var category = await context.ExpenseCategories.FirstAsync(CancellationToken);
        var cash = await context.CashAccounts.FirstAsync(a => a.AccountType == CashAccountType.Cash, CancellationToken);
        context.Expenses.Add(new Expense
        {
            Status = DocumentStatus.Posted,
            Number = null,
            DocumentDate = DateTime.Now,
            ExpenseCategoryId = category.Id,
            Amount = 1000m,
            PaymentMethod = PaymentMethod.Cash,
            CashAccountId = cash.Id,
            Description = "اختبار",
        });

        await Should.ThrowAsync<DbUpdateException>(() => context.SaveChangesAsync(CancellationToken));
    }

    [Fact]
    public async Task DeletingReferencedCategory_IsRestricted()
    {
        int categoryId;
        await using (var setup = fixture.Database.CreateContext())
        {
            var category = new Category { Name = "تصنيف " + Guid.NewGuid().ToString("N")[..8] };
            setup.Categories.Add(category);
            await setup.SaveChangesAsync(CancellationToken);
            var product = await TestData.AddProductAsync(setup);
            product.CategoryId = category.Id;
            await setup.SaveChangesAsync(CancellationToken);
            categoryId = category.Id;
        }

        // A fresh context knows nothing about the product, so the database FK itself must refuse the delete.
        await using var context = fixture.Database.CreateContext();
        context.Categories.Remove(await context.Categories.SingleAsync(c => c.Id == categoryId, CancellationToken));
        var exception = await Should.ThrowAsync<DbUpdateException>(() => context.SaveChangesAsync(CancellationToken));

        SqlErrorTranslator.TryTranslate(exception, out var error).ShouldBeTrue();
        error.Code.ShouldBe("Database.Reference");
    }
}

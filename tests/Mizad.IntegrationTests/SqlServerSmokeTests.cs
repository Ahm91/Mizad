using System.Globalization;
using Microsoft.Data.SqlClient;
using Mizad.Application.Abstractions.Configuration;
using Mizad.Infrastructure.Configuration;

namespace Mizad.IntegrationTests;

/// <summary>Phase 1 smoke tests: the test server is usable and our connection-string factory connects.</summary>
public sealed class SqlServerSmokeTests(SqlServerFixture fixture)
{
    [Fact]
    public async Task TestDatabase_IsCreatedWithArabicCollation()
    {
        var collation = await ScalarAsync(fixture.ConnectionString, "SELECT CONVERT(nvarchar(128), DATABASEPROPERTYEX(DB_NAME(), 'Collation'))");

        collation.ShouldBe(SqlServerFixture.DatabaseCollation);
    }

    [Fact]
    public async Task Server_IsSqlServer2019OrNewer()
    {
        var majorVersion = await ScalarAsync(fixture.ConnectionString, "SELECT CONVERT(int, SERVERPROPERTY('ProductMajorVersion'))");

        Convert.ToInt32(majorVersion, CultureInfo.InvariantCulture).ShouldBeGreaterThanOrEqualTo(15);
    }

    [Fact]
    public async Task ConnectionStringFactory_ProducesAWorkingConnection()
    {
        var source = new SqlConnectionStringBuilder(fixture.ConnectionString);
        var settings = new ConnectionSettings
        {
            Server = source.DataSource,
            Database = fixture.DatabaseName,
            AuthenticationMode = source.IntegratedSecurity ? SqlAuthenticationMode.Windows : SqlAuthenticationMode.SqlServer,
            UserName = source.IntegratedSecurity ? null : source.UserID,
            Password = source.IntegratedSecurity ? null : source.Password,
            TrustServerCertificate = source.TrustServerCertificate,
        };

        var connectionString = new SqlConnectionStringFactory().Create(settings);
        var databaseName = await ScalarAsync(connectionString, "SELECT DB_NAME()");

        databaseName.ShouldBe(fixture.DatabaseName);
    }

    [Fact]
    public async Task ArabicCollation_IsCaseInsensitiveForLatinCodes()
    {
        var result = await ScalarAsync(fixture.ConnectionString, "SELECT CASE WHEN N'sku-abc' = N'SKU-ABC' THEN 1 ELSE 0 END");

        Convert.ToInt32(result, CultureInfo.InvariantCulture).ShouldBe(1);
    }

    [Theory]
    [InlineData("مُحَمَّد", "محمد")] // tashkeel
    [InlineData("أحمد", "احمد")] // hamza on alef
    [InlineData("مكتبة", "مكتبه")] // taa marbuta
    public async Task ArabicCollation_DoesNotNormalizeArabicSpelling_SearchMustUseSearchName(string written, string searched)
    {
        // Verified 2026-11-07 (D-041 revised): no SQL Server collation equates these forms, so Arabic-tolerant
        // search must compare the application-normalized SearchName column, never raw names.
        await using var connection = new SqlConnection(fixture.ConnectionString);
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        await using var command = new SqlCommand("SELECT CASE WHEN @a = @b THEN 1 ELSE 0 END", connection);
        command.Parameters.AddWithValue("@a", written);
        command.Parameters.AddWithValue("@b", searched);

        var result = await command.ExecuteScalarAsync(TestContext.Current.CancellationToken);

        Convert.ToInt32(result, CultureInfo.InvariantCulture).ShouldBe(0);
    }

    private static async Task<object?> ScalarAsync(string connectionString, string sql)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
#pragma warning disable CA2100 // Constant test SQL.
        command.CommandText = sql;
#pragma warning restore CA2100
        return await command.ExecuteScalarAsync(cancellationToken);
    }
}

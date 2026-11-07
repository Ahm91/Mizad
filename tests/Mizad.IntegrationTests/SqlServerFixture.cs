using Microsoft.Data.SqlClient;

[assembly: AssemblyFixture(typeof(Mizad.IntegrationTests.SqlServerFixture))]

namespace Mizad.IntegrationTests;

/// <summary>
/// Creates one temporary database <c>Mizad_IT_&lt;guid&gt;</c> (collation Arabic_100_CI_AI, D-041) per test run
/// and drops it afterwards (TEST_PLAN §1). If SQL Server is unreachable the run FAILS with a clear message.
/// </summary>
public sealed class SqlServerFixture : IAsyncLifetime
{
    public const string ConnectionEnvironmentVariable = "MIZAD_TEST_SQL";
    public const string DatabaseCollation = "Arabic_100_CI_AI";

    private const string DefaultServerConnection = "Server=.;Integrated Security=True;TrustServerCertificate=True";

    public SqlServerFixture()
    {
        var configured = Environment.GetEnvironmentVariable(ConnectionEnvironmentVariable);
        var serverBuilder = new SqlConnectionStringBuilder(string.IsNullOrWhiteSpace(configured) ? DefaultServerConnection : configured)
        {
            InitialCatalog = "master",
            ApplicationName = "Mizad.IntegrationTests",
        };

        ServerConnectionString = serverBuilder.ConnectionString;
        DatabaseName = "Mizad_IT_" + Guid.NewGuid().ToString("N");
        ConnectionString = new SqlConnectionStringBuilder(ServerConnectionString) { InitialCatalog = DatabaseName }.ConnectionString;
    }

    /// <summary>Connection to <c>master</c> on the test server.</summary>
    public string ServerConnectionString { get; }

    /// <summary>Generated name — safe to embed in DDL (letters, digits and underscore only).</summary>
    public string DatabaseName { get; }

    /// <summary>Connection to the temporary test database.</summary>
    public string ConnectionString { get; }

    public async ValueTask InitializeAsync()
    {
        try
        {
            await ExecuteOnServerAsync($"CREATE DATABASE [{DatabaseName}] COLLATE {DatabaseCollation};");
        }
        catch (SqlException ex)
        {
            throw new InvalidOperationException(
                $"SQL Server is not reachable for integration tests. Set {ConnectionEnvironmentVariable} " +
                "to a SQL Server connection string (database is ignored) or start the local default instance.",
                ex);
        }
    }

    public async ValueTask DisposeAsync()
    {
        SqlConnection.ClearAllPools();
        await ExecuteOnServerAsync(
            $"IF DB_ID(N'{DatabaseName}') IS NOT NULL BEGIN " +
            $"ALTER DATABASE [{DatabaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; " +
            $"DROP DATABASE [{DatabaseName}]; END");
    }

    private async Task ExecuteOnServerAsync(string sql)
    {
        await using var connection = new SqlConnection(ServerConnectionString);
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        await using var command = connection.CreateCommand();
#pragma warning disable CA2100 // DDL with a generated identifier (Mizad_IT_<guid>), never user input.
        command.CommandText = sql;
#pragma warning restore CA2100
        await command.ExecuteNonQueryAsync(TestContext.Current.CancellationToken);
    }
}

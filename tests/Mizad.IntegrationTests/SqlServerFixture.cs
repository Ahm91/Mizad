[assembly: AssemblyFixture(typeof(Mizad.IntegrationTests.SqlServerFixture))]

namespace Mizad.IntegrationTests;

/// <summary>
/// Shared database for the run: created, migrated and seeded through the PRODUCTION initializer (D-043), then
/// dropped. Tests that need a pristine or special database create their own <see cref="TestDatabase"/>.
/// If SQL Server is unreachable the run FAILS with a clear message (TEST_PLAN §1).
/// </summary>
public sealed class SqlServerFixture : IAsyncLifetime
{
    public const string DatabaseCollation = "Arabic_100_CI_AI";

    public TestDatabase Database { get; } = new();

    public string ConnectionString => Database.ConnectionString;

    public string DatabaseName => Database.Name;

    public ValueTask InitializeAsync() => new(Database.EnsureReadyAsync());

    public ValueTask DisposeAsync() => Database.DisposeAsync();
}

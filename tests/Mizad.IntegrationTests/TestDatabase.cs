using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Mizad.Application.Abstractions.Database;
using Mizad.Infrastructure;
using Mizad.Infrastructure.Backup;
using Mizad.Infrastructure.Configuration;
using Mizad.Infrastructure.Persistence;

namespace Mizad.IntegrationTests;

/// <summary>
/// A temporary database <c>Mizad_IT_&lt;guid&gt;</c> on the test server plus a service provider wired exactly like
/// production (AddInfrastructure) but pointing at that database. Dropping it also removes its backup files.
/// Server: <c>MIZAD_TEST_SQL</c> or the local default instance (D-036).
/// </summary>
public sealed class TestDatabase : IAsyncDisposable
{
    public const string ConnectionEnvironmentVariable = "MIZAD_TEST_SQL";

    private const string DefaultServerConnection = "Server=.;Integrated Security=True;TrustServerCertificate=True";

    private readonly string _paths = Path.Combine(Path.GetTempPath(), "MizadTests", Guid.NewGuid().ToString("N"));

    public TestDatabase(string? backupDirectoryOverride = null, string? connectionStringOverride = null)
    {
        Name = "Mizad_IT_" + Guid.NewGuid().ToString("N");
        ConnectionString = connectionStringOverride
            ?? new SqlConnectionStringBuilder(ServerConnectionString) { InitialCatalog = Name, ApplicationName = "Mizad.IntegrationTests" }.ConnectionString;
        Services = BuildServices(ConnectionString, backupDirectoryOverride);
    }

    public static string ServerConnectionString { get; } = new SqlConnectionStringBuilder(
        Environment.GetEnvironmentVariable(ConnectionEnvironmentVariable) is { Length: > 0 } configured ? configured : DefaultServerConnection)
    {
        InitialCatalog = "master",
        ApplicationName = "Mizad.IntegrationTests",
    }.ConnectionString;

    public string Name { get; }

    public string ConnectionString { get; }

    public ServiceProvider Services { get; }

    public T Get<T>()
        where T : notnull => Services.GetRequiredService<T>();

    public MizadDbContext CreateContext() => Get<IDbContextFactory<MizadDbContext>>().CreateDbContext();

    /// <summary>Creates and migrates the database through the production initializer (D-043).</summary>
    public async Task EnsureReadyAsync()
    {
        var result = await Get<IDatabaseInitializer>().EnsureReadyAsync(null, TestContext.Current.CancellationToken);
        if (result.IsFailure)
        {
            throw new InvalidOperationException(
                $"Test database could not be prepared on '{ServerConnectionString}'. Set {ConnectionEnvironmentVariable} or start the " +
                $"local SQL Server. {result.Error.Code}: {result.Error.Message}");
        }
    }

    /// <summary>Creates an EMPTY database (no migrations) with the Arabic collation — simulates an outdated database.</summary>
    public Task CreateEmptyAsync() =>
        ExecuteOnServerAsync($"CREATE DATABASE [{Name}] COLLATE {DatabaseInitializer.RequiredCollation};");

    public static async Task<object?> ScalarAsync(string connectionString, string sql, params (string Name, object Value)[] parameters)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        await using var command = connection.CreateCommand();
#pragma warning disable CA2100 // Test SQL; values are parameters.
        command.CommandText = sql;
#pragma warning restore CA2100
        foreach (var (name, value) in parameters)
        {
            command.Parameters.AddWithValue(name, value);
        }

        var result = await command.ExecuteScalarAsync(TestContext.Current.CancellationToken);
        return result is DBNull ? null : result;
    }

    /// <summary>Number of COPY_ONLY backups SQL Server recorded for this database (pre-migration backups).</summary>
    public async Task<int> CountCopyOnlyBackupsAsync() => Convert.ToInt32(
        await ScalarAsync(
            ServerConnectionString,
            "SELECT COUNT(*) FROM msdb.dbo.backupset WHERE database_name = @db AND is_copy_only = 1;",
            ("@db", Name)),
        System.Globalization.CultureInfo.InvariantCulture);

    public async ValueTask DisposeAsync()
    {
        await Services.DisposeAsync();
        SqlConnection.ClearAllPools();
        await DeleteBackupFilesAsync();
        await ExecuteOnServerAsync(
            $"IF DB_ID(N'{Name}') IS NOT NULL BEGIN " +
            $"ALTER DATABASE [{Name}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{Name}]; END");

        if (Directory.Exists(_paths))
        {
            Directory.Delete(_paths, recursive: true);
        }
    }

    private ServiceProvider BuildServices(string connectionString, string? backupDirectoryOverride)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddInfrastructure(new MizadPaths(Path.Combine(_paths, "machine"), Path.Combine(_paths, "user")));
        services.AddSingleton<Mizad.Application.Abstractions.Database.IConnectionStringProvider>(new StaticConnectionStringProvider(connectionString));
        services.AddSingleton(new BackupOptions { DirectoryOverride = backupDirectoryOverride });
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
    }

    private async Task DeleteBackupFilesAsync()
    {
        // Backup files live in the SQL Server backup folder (owned by the service account), so delete them through SQL.
        try
        {
            await using var connection = new SqlConnection(ServerConnectionString);
            await connection.OpenAsync();
            var files = new List<string>();
            await using (var query = connection.CreateCommand())
            {
                query.CommandText =
                    "SELECT DISTINCT m.physical_device_name FROM msdb.dbo.backupset b " +
                    "JOIN msdb.dbo.backupmediafamily m ON b.media_set_id = m.media_set_id WHERE b.database_name = @db;";
                query.Parameters.AddWithValue("@db", Name);
                await using var reader = await query.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    files.Add(reader.GetString(0));
                }
            }

            foreach (var file in files)
            {
                await using var delete = connection.CreateCommand();
                delete.CommandText = "EXEC master.sys.xp_delete_files @file;";
                delete.Parameters.AddWithValue("@file", file);
                await delete.ExecuteNonQueryAsync();
            }

            await using var history = connection.CreateCommand();
            history.CommandText = "EXEC msdb.dbo.sp_delete_database_backuphistory @database_name = @db;";
            history.Parameters.AddWithValue("@db", Name);
            await history.ExecuteNonQueryAsync();
        }
        catch (SqlException)
        {
            // Best effort: leftover test backups are harmless.
        }
    }

    private static async Task ExecuteOnServerAsync(string sql)
    {
        await using var connection = new SqlConnection(ServerConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
#pragma warning disable CA2100 // DDL with a generated identifier (Mizad_IT_<guid>), never user input.
        command.CommandText = sql;
#pragma warning restore CA2100
        command.CommandTimeout = 120;
        await command.ExecuteNonQueryAsync();
    }
}

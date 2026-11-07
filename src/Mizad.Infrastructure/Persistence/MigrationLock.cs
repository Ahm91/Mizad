using System.Data;
using Microsoft.Data.SqlClient;

namespace Mizad.Infrastructure.Persistence;

/// <summary>
/// Database-wide application lock (task 2.21): only one device creates/migrates/seeds at a time. The lock is owned
/// by a dedicated session and released when disposed (or automatically if the process dies).
/// </summary>
internal sealed class MigrationLock : IAsyncDisposable
{
    public const string MigrationResource = "Mizad.Migrations";

    private readonly SqlConnection _connection;
    private readonly string _resource;

    private MigrationLock(SqlConnection connection, string resource)
    {
        _connection = connection;
        _resource = resource;
    }

    /// <summary>Returns the lock, or null if it could not be acquired within <paramref name="timeout"/>.</summary>
    public static async Task<MigrationLock?> TryAcquireAsync(
        string connectionString, string resource, TimeSpan timeout, CancellationToken cancellationToken)
    {
        var connection = new SqlConnection(connectionString);
        try
        {
            await connection.OpenAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = "sp_getapplock";
            command.CommandType = CommandType.StoredProcedure;
            command.CommandTimeout = (int)Math.Ceiling(timeout.TotalSeconds) + 30;
            command.Parameters.AddWithValue("@Resource", resource);
            command.Parameters.AddWithValue("@LockMode", "Exclusive");
            command.Parameters.AddWithValue("@LockOwner", "Session");
            command.Parameters.AddWithValue("@LockTimeout", (int)timeout.TotalMilliseconds);
            var returnValue = command.Parameters.Add("@ReturnValue", SqlDbType.Int);
            returnValue.Direction = ParameterDirection.ReturnValue;
            await command.ExecuteNonQueryAsync(cancellationToken);

            // 0 = granted, 1 = granted after waiting; negative = timeout/cancel/deadlock/error.
            if ((int)returnValue.Value >= 0)
            {
                return new MigrationLock(connection, resource);
            }
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }

        await connection.DisposeAsync();
        return null;
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_connection.State == ConnectionState.Open)
            {
                await using var command = _connection.CreateCommand();
                command.CommandText = "sp_releaseapplock";
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@Resource", _resource);
                command.Parameters.AddWithValue("@LockOwner", "Session");
                await command.ExecuteNonQueryAsync();
            }
        }
        catch (SqlException)
        {
            // Closing the session releases the lock anyway.
        }
        finally
        {
            await _connection.DisposeAsync();
        }
    }
}

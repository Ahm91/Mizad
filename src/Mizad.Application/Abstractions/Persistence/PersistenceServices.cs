using System.Data;
using Mizad.Domain.Administration;

namespace Mizad.Application.Abstractions.Persistence;

/// <summary>
/// Runs a use case inside one SQL transaction with a fresh <see cref="IMizadDbContext"/> per attempt, wrapped in
/// the provider's retry strategy (task 2.15, BR-002). The operation must be idempotent across retries — which
/// posting is, thanks to ClientRequestId (D-011). Changes are saved before commit.
/// </summary>
public interface IUnitOfWork
{
    Task<T> ExecuteInTransactionAsync<T>(
        Func<IMizadDbContext, CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

    /// <summary>Runs a read-only operation with a fresh context (no transaction, no save).</summary>
    Task<T> ReadAsync<T>(Func<IMizadDbContext, CancellationToken, Task<T>> query, CancellationToken cancellationToken);
}

/// <summary>Allocates gap-free official numbers inside the caller's transaction (D-010, task 2.16).</summary>
public interface ISequenceAllocator
{
    /// <summary>Returns the formatted next number (e.g. <c>SB-000001</c>) for <paramref name="documentType"/>.</summary>
    Task<string> NextAsync(IMizadDbContext context, string documentType, CancellationToken cancellationToken);
}

/// <summary>The SQL Server clock — used for posting timestamps so machines with wrong clocks agree (D-023).</summary>
public interface IServerClock
{
    Task<DateTime> NowAsync(IMizadDbContext context, CancellationToken cancellationToken);
}

/// <summary>Typed access to shared shop settings with a short-lived cache (task 2.17).</summary>
public interface ISettingsService
{
    Task<T> GetAsync<T>(SettingDefinition<T> definition, CancellationToken cancellationToken);

    /// <summary>Saves a value, audits the change and refreshes the cache. Fails if the setting is locked.</summary>
    Task<Domain.Common.Result> SetAsync<T>(SettingDefinition<T> definition, T value, CancellationToken cancellationToken);

    void InvalidateCache();
}

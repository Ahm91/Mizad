using System.Data;
using System.Data.Common;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Mizad.Application.Abstractions;
using Mizad.Application.Abstractions.Persistence;
using Mizad.Domain.Administration;
using Mizad.Domain.Common;
using Mizad.Domain.Identity;

namespace Mizad.Infrastructure.Persistence;

/// <summary>Transaction + retry strategy with a fresh context per attempt (task 2.15, BR-002).</summary>
internal sealed class UnitOfWork(IDbContextFactory<MizadDbContext> contextFactory) : IUnitOfWork
{
    public async Task<T> ExecuteInTransactionAsync<T>(
        Func<IMizadDbContext, CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        ArgumentNullException.ThrowIfNull(operation);

        await using var strategyContext = contextFactory.CreateDbContext();
        var strategy = strategyContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(
            async token =>
            {
                await using var context = contextFactory.CreateDbContext();
                await using var transaction = await context.Database.BeginTransactionAsync(isolationLevel, token);
                var result = await operation(context, token);
                await context.SaveChangesAsync(token);
                await transaction.CommitAsync(token);
                return result;
            },
            cancellationToken);
    }

    public async Task<T> ReadAsync<T>(Func<IMizadDbContext, CancellationToken, Task<T>> query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        await using var context = contextFactory.CreateDbContext();
        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        return await query(context, cancellationToken);
    }
}

/// <summary>Helpers to run raw ADO commands on the context's connection inside its current transaction.</summary>
internal static class ContextCommands
{
    public static async Task<DbCommand> CreateCommandAsync(IMizadDbContext context, string sql, CancellationToken cancellationToken)
    {
        var database = context.Database;
        if (database.GetDbConnection().State != ConnectionState.Open)
        {
            await database.OpenConnectionAsync(cancellationToken);
        }

        var command = database.GetDbConnection().CreateCommand();
#pragma warning disable CA2100 // Callers pass constant SQL; values are always parameters.
        command.CommandText = sql;
#pragma warning restore CA2100
        command.Transaction = database.CurrentTransaction?.GetDbTransaction();
        return command;
    }

    public static void AddParameter(DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }
}

/// <summary>
/// Gap-free numbering (D-010): the sequence row is locked by the UPDATE until the posting transaction ends,
/// so a rolled-back posting releases its number and concurrent postings serialize on that row only.
/// </summary>
internal sealed class SequenceAllocator : ISequenceAllocator
{
    private const string Sql =
        "UPDATE dbo.DocumentSequences WITH (UPDLOCK, ROWLOCK) SET NextNumber = NextNumber + 1 " +
        "OUTPUT deleted.NextNumber, deleted.Prefix, deleted.Padding WHERE DocumentType = @type;";

    public async Task<string> NextAsync(IMizadDbContext context, string documentType, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentException.ThrowIfNullOrWhiteSpace(documentType);

        if (context.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("Document numbers must be allocated inside the posting transaction (D-010).");
        }

        await using var command = await ContextCommands.CreateCommandAsync(context, Sql, cancellationToken);
        ContextCommands.AddParameter(command, "@type", documentType);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            throw new InvalidOperationException($"Document sequence '{documentType}' is not defined. Run the seed sync.");
        }

        return DocumentSequence.Format(reader.GetString(1), reader.GetInt32(0), reader.GetByte(2));
    }
}

/// <summary>SQL Server clock (D-023).</summary>
internal sealed class ServerClock : IServerClock
{
    public async Task<DateTime> NowAsync(IMizadDbContext context, CancellationToken cancellationToken)
    {
        await using var command = await ContextCommands.CreateCommandAsync(context, "SELECT SYSDATETIME();", cancellationToken);
        var value = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToDateTime(value, CultureInfo.InvariantCulture);
    }
}

/// <summary>Adds audit rows to the caller's context so they commit with the business change (D-037).</summary>
internal sealed class AuditLogger(IClock clock, ICurrentUser currentUser) : IAuditLogger
{
    private const int SummaryMaxLength = 500;

    public void Log(IMizadDbContext context, AuditEntry entry)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(entry);

        context.AuditLogs.Add(new AuditLog
        {
            OccurredAtUtc = clock.UtcNow,
            UserId = currentUser.UserId,
            UserName = currentUser.UserName,
            MachineName = Environment.MachineName,
            Action = entry.Action,
            EntityType = entry.EntityType,
            EntityId = entry.EntityId,
            Summary = entry.Summary.Length > SummaryMaxLength ? entry.Summary[..SummaryMaxLength] : entry.Summary,
            DetailsJson = entry.DetailsJson,
        });
    }
}

/// <summary>
/// Shared settings with a 60-second cache (task 2.17). Changes are audited. <c>Currency.Decimals</c> is locked once
/// any document has been posted, because changing it would break historical rounding (D-013).
/// </summary>
internal sealed class SettingsService(IUnitOfWork unitOfWork, IAuditLogger auditLogger, IClock clock, ICurrentUser currentUser)
    : ISettingsService
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(60);
    private readonly Lock _gate = new();
    private Dictionary<string, string?>? _cache;
    private DateTime _cacheLoadedAtUtc;

    public async Task<T> GetAsync<T>(SettingDefinition<T> definition, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(definition);
        var values = await GetAllAsync(cancellationToken);
        return values.TryGetValue(definition.Key, out var text)
            ? SettingValue.Parse(text, definition.Default)
            : definition.Default;
    }

    public async Task<Result> SetAsync<T>(SettingDefinition<T> definition, T value, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(definition);

        var result = await unitOfWork.ExecuteInTransactionAsync(
            async (context, token) =>
            {
                if (definition.Key == SettingKeys.CurrencyDecimals.Key && await PostedDocumentQueries.AnyPostedAsync(context, token))
                {
                    return Result.Failure(Error.BusinessRule(
                        "Settings.CurrencyDecimalsLocked",
                        "لا يمكن تغيير عدد المنازل العشرية للعملة بعد اعتماد أول مستند."));
                }

                var text = SettingValue.Format(value);
                var setting = await context.Settings.FirstOrDefaultAsync(s => s.Key == definition.Key, token);
                var oldText = setting?.Value;
                if (setting is null)
                {
                    setting = new Setting { Key = definition.Key };
                    context.Settings.Add(setting);
                }

                setting.Value = text;
                setting.UpdatedAtUtc = clock.UtcNow;
                setting.UpdatedByUserId = currentUser.UserId;
                auditLogger.Log(context, new AuditEntry(
                    "Settings.Changed",
                    $"تغيير الإعداد {definition.Key}",
                    nameof(Setting),
                    definition.Key,
                    System.Text.Json.JsonSerializer.Serialize(new { Old = oldText, New = text })));
                return Result.Success();
            },
            cancellationToken);

        InvalidateCache();
        return result;
    }

    public void InvalidateCache()
    {
        lock (_gate)
        {
            _cache = null;
        }
    }

    private async Task<Dictionary<string, string?>> GetAllAsync(CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            if (_cache is not null && clock.UtcNow - _cacheLoadedAtUtc < CacheDuration)
            {
                return _cache;
            }
        }

        var values = await unitOfWork.ReadAsync(
            (context, token) => context.Settings.ToDictionaryAsync(s => s.Key, s => s.Value, token),
            cancellationToken);

        lock (_gate)
        {
            _cache = values;
            _cacheLoadedAtUtc = clock.UtcNow;
        }

        return values;
    }
}

/// <summary>Detects whether any document has ever been posted (used by locks such as currency decimals).</summary>
internal static class PostedDocumentQueries
{
    public static async Task<bool> AnyPostedAsync(IMizadDbContext context, CancellationToken cancellationToken) =>
        await context.SalesInvoices.AnyAsync(x => x.Status != DocumentStatus.Draft, cancellationToken)
        || await context.PurchaseInvoices.AnyAsync(x => x.Status != DocumentStatus.Draft, cancellationToken)
        || await context.SalesReturns.AnyAsync(x => x.Status != DocumentStatus.Draft, cancellationToken)
        || await context.PurchaseReturns.AnyAsync(x => x.Status != DocumentStatus.Draft, cancellationToken)
        || await context.CashVouchers.AnyAsync(x => x.Status != DocumentStatus.Draft, cancellationToken)
        || await context.Expenses.AnyAsync(x => x.Status != DocumentStatus.Draft, cancellationToken)
        || await context.CashTransfers.AnyAsync(x => x.Status != DocumentStatus.Draft, cancellationToken)
        || await context.OpeningBalances.AnyAsync(x => x.Status != DocumentStatus.Draft, cancellationToken)
        || await context.StockAdjustments.AnyAsync(x => x.Status != DocumentStatus.Draft, cancellationToken)
        || await context.StockTransfers.AnyAsync(x => x.Status != DocumentStatus.Draft, cancellationToken);
}

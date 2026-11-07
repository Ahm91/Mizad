using Mizad.Application.Abstractions.Persistence;

namespace Mizad.Application.Abstractions;

/// <summary>A sensitive operation to record in AuditLogs (D-037). Never include passwords or secrets.</summary>
public sealed record AuditEntry(
    string Action,
    string Summary,
    string? EntityType = null,
    string? EntityId = null,
    string? DetailsJson = null);

/// <summary>
/// Adds audit entries to the given context so they are committed atomically with the business operation
/// (D-037). The caller saves the context.
/// </summary>
public interface IAuditLogger
{
    void Log(IMizadDbContext context, AuditEntry entry);
}

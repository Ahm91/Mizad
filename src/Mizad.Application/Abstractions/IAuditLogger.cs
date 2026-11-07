namespace Mizad.Application.Abstractions;

/// <summary>A sensitive operation to record in AuditLogs (D-037). Never include passwords or secrets.</summary>
public sealed record AuditEntry(
    string Action,
    string Summary,
    string? EntityType = null,
    string? EntityId = null,
    string? DetailsJson = null);

/// <summary>Writes audit entries inside the current business transaction (implemented in task 3.7).</summary>
public interface IAuditLogger
{
    Task LogAsync(AuditEntry entry, CancellationToken cancellationToken);
}

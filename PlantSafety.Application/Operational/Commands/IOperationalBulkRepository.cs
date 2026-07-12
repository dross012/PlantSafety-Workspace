// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-13
// FILE DESCRIPTION: Repository abstraction for bulk Operational resource operations.
// SchemaVersion: 1

namespace PlantSafety.Application.Operational.Commands;

/// <summary>Applies bulk operations across Operational resources. Implemented in the Infrastructure layer.</summary>
public interface IOperationalBulkRepository
{
    /// <summary>
    /// Applies <paramref name="operation"/> to every resource in <paramref name="resourceIds"/>, returning
    /// one result per resource. Must not throw for individual item failures — those are reported per-item.
    /// </summary>
    Task<IReadOnlyList<BulkOperationalItemResult>> BulkUpdateAsync(
        Guid tenantId,
        Guid? siteId,
        IReadOnlyList<Guid> resourceIds,
        string operation,
        Dictionary<string, string>? parameters,
        CancellationToken cancellationToken);
}

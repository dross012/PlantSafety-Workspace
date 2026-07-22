// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for bulk Scheduling resource operations.
// SchemaVersion: 1

namespace PlantSafety.Application.Scheduling.Commands;

/// <summary>Applies bulk operations across Scheduling resources. Implemented in the Infrastructure layer.</summary>
public interface ISchedulingBulkRepository
{
    /// <summary>
    /// Applies <paramref name="operation"/> to every resource in <paramref name="resourceIds"/>, returning
    /// one result per resource. Must not throw for individual item failures - those are reported per-item.
    /// </summary>
    Task<IReadOnlyList<BulkSchedulingItemResult>> BulkUpdateAsync(
        Guid tenantId,
        Guid? siteId,
        IReadOnlyList<Guid> resourceIds,
        string operation,
        Dictionary<string, string>? parameters,
        CancellationToken cancellationToken);
}
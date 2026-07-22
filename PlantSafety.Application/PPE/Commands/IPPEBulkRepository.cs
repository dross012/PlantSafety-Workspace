// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for bulk PPE resource operations.
// SchemaVersion: 1

namespace PlantSafety.Application.PPE.Commands;

/// <summary>Applies bulk operations across PPE resources. Implemented in the Infrastructure layer.</summary>
public interface IPPEBulkRepository
{
    /// <summary>
    /// Applies <paramref name="operation"/> to every resource in <paramref name="resourceIds"/>, returning
    /// one result per resource. Must not throw for individual item failures - those are reported per-item.
    /// </summary>
    Task<IReadOnlyList<BulkPPEItemResult>> BulkUpdateAsync(
        Guid tenantId,
        Guid? siteId,
        IReadOnlyList<Guid> resourceIds,
        string operation,
        Dictionary<string, string>? parameters,
        CancellationToken cancellationToken);
}
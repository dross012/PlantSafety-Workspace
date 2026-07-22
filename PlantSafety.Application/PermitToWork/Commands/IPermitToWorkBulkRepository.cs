// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for bulk PermitToWork resource operations.
// SchemaVersion: 1

namespace PlantSafety.Application.PermitToWork.Commands;

/// <summary>Applies bulk operations across PermitToWork resources. Implemented in the Infrastructure layer.</summary>
public interface IPermitToWorkBulkRepository
{
    /// <summary>
    /// Applies <paramref name="operation"/> to every resource in <paramref name="resourceIds"/>, returning
    /// one result per resource. Must not throw for individual item failures - those are reported per-item.
    /// </summary>
    Task<IReadOnlyList<BulkPermitToWorkItemResult>> BulkUpdateAsync(
        Guid tenantId,
        Guid? siteId,
        IReadOnlyList<Guid> resourceIds,
        string operation,
        Dictionary<string, string>? parameters,
        CancellationToken cancellationToken);
}
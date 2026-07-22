// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the PPEAssignment read-model.
// SchemaVersion: 1

using PlantSafety.Application.PPE.PPEAssignment.Models;

namespace PlantSafety.Application.PPE.PPEAssignment.Repositories;

/// <summary>Read-only repository for PPEAssignment data. Implemented in the Infrastructure layer.</summary>
public interface IPPEAssignmentRepository
{
    /// <summary>Returns PPEAssignment items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<PPEAssignmentItemDto>> GetPPEAssignmentAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
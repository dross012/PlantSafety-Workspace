// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the WorkInstructions read-model.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Procedures.WorkInstructions.Models;

namespace PlantSafety.Application.Compliance.Procedures.WorkInstructions.Repositories;

/// <summary>Read-only repository for WorkInstructions data. Implemented in the Infrastructure layer.</summary>
public interface IWorkInstructionsRepository
{
    /// <summary>Returns WorkInstructions items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<WorkInstructionsItemDto>> GetWorkInstructionsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}

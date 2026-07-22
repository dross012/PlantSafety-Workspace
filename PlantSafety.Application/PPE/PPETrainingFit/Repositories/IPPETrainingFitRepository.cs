// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the PPETrainingFit read-model.
// SchemaVersion: 1

using PlantSafety.Application.PPE.PPETrainingFit.Models;

namespace PlantSafety.Application.PPE.PPETrainingFit.Repositories;

/// <summary>Read-only repository for PPETrainingFit data. Implemented in the Infrastructure layer.</summary>
public interface IPPETrainingFitRepository
{
    /// <summary>Returns PPETrainingFit items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<PPETrainingFitItemDto>> GetPPETrainingFitAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
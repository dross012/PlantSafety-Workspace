// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the HazardAssessments read-model.
// SchemaVersion: 1

using PlantSafety.Application.Operational.HazardAssessments.Models;

namespace PlantSafety.Application.Operational.HazardAssessments.Repositories;

/// <summary>Read-only repository for HazardAssessments data. Implemented in the Infrastructure layer.</summary>
public interface IHazardAssessmentsRepository
{
    /// <summary>Returns HazardAssessments items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<HazardAssessmentsItemDto>> GetHazardAssessmentsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}

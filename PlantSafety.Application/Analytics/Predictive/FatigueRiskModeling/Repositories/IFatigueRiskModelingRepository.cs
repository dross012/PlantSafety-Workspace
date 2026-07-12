// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the FatigueRiskModeling read-model.
// SchemaVersion: 1

using PlantSafety.Application.Analytics.Predictive.FatigueRiskModeling.Models;

namespace PlantSafety.Application.Analytics.Predictive.FatigueRiskModeling.Repositories;

/// <summary>Read-only repository for FatigueRiskModeling data. Implemented in the Infrastructure layer.</summary>
public interface IFatigueRiskModelingRepository
{
    /// <summary>Returns FatigueRiskModeling items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<FatigueRiskModelingItemDto>> GetFatigueRiskAssessmentsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}

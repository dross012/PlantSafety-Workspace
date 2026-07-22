// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the IncidentTrackingPerformanceEvaluation read-model.
// SchemaVersion: 1

using PlantSafety.Application.ContractorSafety.IncidentTrackingPerformanceEvaluation.Models;

namespace PlantSafety.Application.ContractorSafety.IncidentTrackingPerformanceEvaluation.Repositories;

/// <summary>Read-only repository for IncidentTrackingPerformanceEvaluation data. Implemented in the Infrastructure layer.</summary>
public interface IIncidentTrackingPerformanceEvaluationRepository
{
    /// <summary>Returns IncidentTrackingPerformanceEvaluation items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<IncidentTrackingPerformanceEvaluationItemDto>> GetIncidentTrackingPerformanceEvaluationItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the RootCauseAnalysis read-model.
// SchemaVersion: 1

using PlantSafety.Application.SafetyEvents.RootCauseAnalysis.Models;

namespace PlantSafety.Application.SafetyEvents.RootCauseAnalysis.Repositories;

/// <summary>Read-only repository for RootCauseAnalysis data. Implemented in the Infrastructure layer.</summary>
public interface IRootCauseAnalysisRepository
{
    /// <summary>Returns RootCauseAnalysis items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<RootCauseAnalysisItemDto>> GetRootCauseAnalysesAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}

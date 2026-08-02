// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// UPDATED: 2026-08-02 (Phase 4 full build-out) -- added GetByIdAsync, backing the new
//          GetRootCauseAnalysisByIdQuery read-before-write path for the RootCauseAnalysis
//          aggregate's lifecycle commands. Distinct from
//          PlantSafety.Domain.SafetyEvents.RootCauseAnalysis.Interfaces.IRootCauseAnalysisRepository
//          (the write-side aggregate repository) -- same simple name, different namespace,
//          mirroring PlantSafety.Application.Compliance.Permits.Repositories.IPermitsRepository's
//          identical CQRS read/write repository separation precedent.
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

    /// <summary>
    /// Returns the full RootCauseAnalysis read-model record for the given identity, scoped to the
    /// given tenant, or <see langword="null"/> if not found or if it belongs to a different tenant.
    /// </summary>
    Task<RootCauseAnalysisDetailDto?> GetByIdAsync(Guid rootCauseAnalysisId, Guid tenantId, CancellationToken cancellationToken);
}

// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the Investigation & Root Cause Analysis read-model.
// SchemaVersion: 1

using PlantSafety.Application.AccidentManagement.InvestigationRootCauseAnalysis.Models;

namespace PlantSafety.Application.AccidentManagement.InvestigationRootCauseAnalysis.Repositories;

/// <summary>Read-only repository for Investigation &amp; Root Cause Analysis data. Implemented in the Infrastructure layer.</summary>
public interface IInvestigationRootCauseAnalysisRepository
{
    /// <summary>Returns Investigation &amp; Root Cause Analysis items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<InvestigationRootCauseAnalysisItemDto>> GetInvestigationRootCauseAnalysisAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
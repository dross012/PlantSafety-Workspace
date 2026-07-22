// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the Scorecard Data Sources (cross-module aggregation) read-model.
// SchemaVersion: 1

using PlantSafety.Application.Scorecards.ScorecardDataSources.Models;

namespace PlantSafety.Application.Scorecards.ScorecardDataSources.Repositories;

/// <summary>Read-only repository for Scorecard Data Sources (cross-module aggregation) data. Implemented in the Infrastructure layer.</summary>
public interface IScorecardDataSourcesRepository
{
    /// <summary>Returns Scorecard Data Sources (cross-module aggregation) items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<ScorecardDataSourcesItemDto>> GetScorecardDataSourcesAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
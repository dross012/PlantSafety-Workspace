// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the Scorecard Structure & Visualization Panels read-model.
// SchemaVersion: 1

using PlantSafety.Application.Scorecards.ScorecardStructureVisualization.Models;

namespace PlantSafety.Application.Scorecards.ScorecardStructureVisualization.Repositories;

/// <summary>Read-only repository for Scorecard Structure &amp; Visualization Panels data. Implemented in the Infrastructure layer.</summary>
public interface IScorecardStructureVisualizationRepository
{
    /// <summary>Returns Scorecard Structure &amp; Visualization Panels items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<ScorecardStructureVisualizationItemDto>> GetScorecardStructureVisualizationAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
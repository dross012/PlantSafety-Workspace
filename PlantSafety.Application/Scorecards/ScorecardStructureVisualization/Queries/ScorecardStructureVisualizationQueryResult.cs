// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetScorecardStructureVisualizationQuery.
// SchemaVersion: 1

using PlantSafety.Application.Scorecards.ScorecardStructureVisualization.Models;

namespace PlantSafety.Application.Scorecards.ScorecardStructureVisualization.Queries;

/// <summary>Result of <see cref="GetScorecardStructureVisualizationQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Scorecard Structure &amp; Visualization Panels items.</param>
public sealed record ScorecardStructureVisualizationQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<ScorecardStructureVisualizationItemDto> Items);
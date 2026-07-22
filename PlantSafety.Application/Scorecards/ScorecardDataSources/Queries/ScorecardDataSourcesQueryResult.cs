// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetScorecardDataSourcesQuery.
// SchemaVersion: 1

using PlantSafety.Application.Scorecards.ScorecardDataSources.Models;

namespace PlantSafety.Application.Scorecards.ScorecardDataSources.Queries;

/// <summary>Result of <see cref="GetScorecardDataSourcesQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Scorecard Data Sources (cross-module aggregation) items.</param>
public sealed record ScorecardDataSourcesQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<ScorecardDataSourcesItemDto> Items);
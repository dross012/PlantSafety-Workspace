// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetLaggingIndicatorsQuery.
// SchemaVersion: 1

using PlantSafety.Application.Scorecards.LaggingIndicators.Models;

namespace PlantSafety.Application.Scorecards.LaggingIndicators.Queries;

/// <summary>Result of <see cref="GetLaggingIndicatorsQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Lagging Indicators (TRIR, DART, Lost Workdays) items.</param>
public sealed record LaggingIndicatorsQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<LaggingIndicatorsItemDto> Items);
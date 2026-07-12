// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetEngagementScoringQuery.
// SchemaVersion: 1

using PlantSafety.Application.Analytics.Culture.EngagementScoring.Models;

namespace PlantSafety.Application.Analytics.Culture.EngagementScoring.Queries;

/// <summary>Result of <see cref="GetEngagementScoringQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching EngagementScoring items.</param>
public sealed record EngagementScoringQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<EngagementScoringItemDto> Items);

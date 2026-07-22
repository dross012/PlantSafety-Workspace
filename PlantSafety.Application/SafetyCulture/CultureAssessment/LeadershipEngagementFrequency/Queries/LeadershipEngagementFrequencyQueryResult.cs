// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetLeadershipEngagementFrequencyQuery.
// SchemaVersion: 1

using PlantSafety.Application.SafetyCulture.CultureAssessment.LeadershipEngagementFrequency.Models;

namespace PlantSafety.Application.SafetyCulture.CultureAssessment.LeadershipEngagementFrequency.Queries;

/// <summary>Result of <see cref="GetLeadershipEngagementFrequencyQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching LeadershipEngagementFrequency items.</param>
public sealed record LeadershipEngagementFrequencyQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<LeadershipEngagementFrequencyItemDto> Items);
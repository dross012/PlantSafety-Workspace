// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-13
// FILE DESCRIPTION: Result payload for GetEngagementQuery.
// SchemaVersion: 1

using PlantSafety.Application.Analytics.Models;

namespace PlantSafety.Application.Analytics.Queries;

/// <summary>Result of <see cref="GetEngagementQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide aggregates.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Data">Engagement analytics data points.</param>
public sealed record EngagementQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<EngagementDataPointDto> Data);

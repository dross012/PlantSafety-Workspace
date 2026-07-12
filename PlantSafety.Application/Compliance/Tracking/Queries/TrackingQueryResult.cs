// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetTrackingQuery.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Tracking.Models;

namespace PlantSafety.Application.Compliance.Tracking.Queries;

/// <summary>Result of <see cref="GetTrackingQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Tracking items.</param>
public sealed record TrackingQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<TrackingItemDto> Items);

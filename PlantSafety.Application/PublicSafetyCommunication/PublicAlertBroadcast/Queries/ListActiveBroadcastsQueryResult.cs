// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Result payload for ListActiveBroadcastsQuery.
// SchemaVersion: 1

using PlantSafety.Application.PublicSafetyCommunication.PublicAlertBroadcast.Models;

namespace PlantSafety.Application.PublicSafetyCommunication.PublicAlertBroadcast.Queries;

/// <summary>Result of <see cref="ListActiveBroadcastsQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching active PublicAlertBroadcast items.</param>
public sealed record ListActiveBroadcastsQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<PublicAlertBroadcastItemDto> Items);

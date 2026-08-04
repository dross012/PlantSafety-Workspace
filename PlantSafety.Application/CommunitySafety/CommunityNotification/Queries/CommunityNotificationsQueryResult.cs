// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Result payload for ListNotificationsByPeriodQuery.
// SchemaVersion: 1

using PlantSafety.Application.CommunitySafety.CommunityNotification.Models;

namespace PlantSafety.Application.CommunitySafety.CommunityNotification.Queries;

/// <summary>Result of <see cref="ListNotificationsByPeriodQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching CommunityNotification items.</param>
public sealed record CommunityNotificationsQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<CommunityNotificationItemDto> Items);

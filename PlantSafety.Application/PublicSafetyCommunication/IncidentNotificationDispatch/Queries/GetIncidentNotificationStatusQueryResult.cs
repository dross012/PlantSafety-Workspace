// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Result payload for GetIncidentNotificationStatusQuery.
// SchemaVersion: 1

using PlantSafety.Application.PublicSafetyCommunication.IncidentNotificationDispatch.Models;

namespace PlantSafety.Application.PublicSafetyCommunication.IncidentNotificationDispatch.Queries;

/// <summary>Result of <see cref="GetIncidentNotificationStatusQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching IncidentNotification items.</param>
public sealed record GetIncidentNotificationStatusQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<IncidentNotificationItemDto> Items);

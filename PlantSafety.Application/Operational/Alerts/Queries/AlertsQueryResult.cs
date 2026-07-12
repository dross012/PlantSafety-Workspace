// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetAlertsQuery.
// SchemaVersion: 1

using PlantSafety.Application.Operational.Alerts.Models;

namespace PlantSafety.Application.Operational.Alerts.Queries;

/// <summary>Result of <see cref="GetAlertsQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Alerts items.</param>
public sealed record AlertsQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<AlertsItemDto> Items);

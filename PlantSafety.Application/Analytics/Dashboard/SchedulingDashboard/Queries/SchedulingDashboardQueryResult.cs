// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-02
// FILE DESCRIPTION: Result payload for GetSchedulingDashboardQuery.
// SchemaVersion: 1

using PlantSafety.Application.Analytics.Dashboard.SchedulingDashboard.Models;

namespace PlantSafety.Application.Analytics.Dashboard.SchedulingDashboard.Queries;

/// <summary>Result of <see cref="GetSchedulingDashboardQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Scheduling dashboard KPI items, one per business date.</param>
public sealed record SchedulingDashboardQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<SchedulingDashboardItemDto> Items);

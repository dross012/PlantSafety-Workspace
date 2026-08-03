// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-02
// FILE DESCRIPTION: Result payload for GetRiskDashboardQuery.
// SchemaVersion: 1

using PlantSafety.Application.Analytics.Dashboard.RiskDashboard.Models;

namespace PlantSafety.Application.Analytics.Dashboard.RiskDashboard.Queries;

/// <summary>Result of <see cref="GetRiskDashboardQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Risk dashboard KPI items, one per business date.</param>
public sealed record RiskDashboardQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<RiskDashboardItemDto> Items);

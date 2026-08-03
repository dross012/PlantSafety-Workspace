// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-02
// FILE DESCRIPTION: Result payload for GetContinuousImprovementDashboardQuery.
// SchemaVersion: 1

using PlantSafety.Application.Analytics.Dashboard.ContinuousImprovementDashboard.Models;

namespace PlantSafety.Application.Analytics.Dashboard.ContinuousImprovementDashboard.Queries;

/// <summary>Result of <see cref="GetContinuousImprovementDashboardQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching ContinuousImprovement dashboard KPI items, one per business date.</param>
public sealed record ContinuousImprovementDashboardQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<ContinuousImprovementDashboardItemDto> Items);

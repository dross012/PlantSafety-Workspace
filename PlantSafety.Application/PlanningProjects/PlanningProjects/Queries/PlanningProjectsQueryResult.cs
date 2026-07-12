// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetPlanningProjectsQuery.
// SchemaVersion: 1

using PlantSafety.Application.PlanningProjects.PlanningProjects.Models;

namespace PlantSafety.Application.PlanningProjects.PlanningProjects.Queries;

/// <summary>Result of <see cref="GetPlanningProjectsQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching PlanningProjects items.</param>
public sealed record PlanningProjectsQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<PlanningProjectItemDto> Items);

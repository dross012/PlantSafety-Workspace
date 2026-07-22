// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetWorkingAtHeightsQuery.
// SchemaVersion: 1

using PlantSafety.Application.Checklists.ChecklistTemplate.WorkingAtHeights.Models;

namespace PlantSafety.Application.Checklists.ChecklistTemplate.WorkingAtHeights.Queries;

/// <summary>Result of <see cref="GetWorkingAtHeightsQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching WorkingAtHeights items.</param>
public sealed record WorkingAtHeightsQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<WorkingAtHeightsItemDto> Items);
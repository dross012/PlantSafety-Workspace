// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetChecklistsQuery.
// SchemaVersion: 1

using PlantSafety.Application.Operational.Checklists.Models;

namespace PlantSafety.Application.Operational.Checklists.Queries;

/// <summary>Result of <see cref="GetChecklistsQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Checklists items.</param>
public sealed record ChecklistsQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<ChecklistsItemDto> Items);

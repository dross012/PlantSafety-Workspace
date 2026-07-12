// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetDepartmentQuery.
// SchemaVersion: 1

using PlantSafety.Application.Analytics.ScoreCards.Department.Models;

namespace PlantSafety.Application.Analytics.ScoreCards.Department.Queries;

/// <summary>Result of <see cref="GetDepartmentQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Department items.</param>
public sealed record DepartmentQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<DepartmentItemDto> Items);

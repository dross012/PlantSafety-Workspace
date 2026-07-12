// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetEmployeesQuery.
// SchemaVersion: 1

using PlantSafety.Application.CrossDomain.Employees.Models;

namespace PlantSafety.Application.CrossDomain.Employees.Queries;

/// <summary>Result of <see cref="GetEmployeesQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Employees items.</param>
public sealed record EmployeesQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<EmployeesItemDto> Items);

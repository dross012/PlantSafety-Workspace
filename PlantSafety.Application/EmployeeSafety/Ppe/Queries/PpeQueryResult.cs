// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetPpeQuery.
// SchemaVersion: 1

using PlantSafety.Application.EmployeeSafety.Ppe.Models;

namespace PlantSafety.Application.EmployeeSafety.Ppe.Queries;

/// <summary>Result of <see cref="GetPpeQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Ppe items.</param>
public sealed record PpeQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<PpeItemDto> Items);
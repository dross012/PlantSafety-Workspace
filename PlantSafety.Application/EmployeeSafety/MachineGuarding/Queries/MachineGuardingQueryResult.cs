// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetMachineGuardingQuery.
// SchemaVersion: 1

using PlantSafety.Application.EmployeeSafety.MachineGuarding.Models;

namespace PlantSafety.Application.EmployeeSafety.MachineGuarding.Queries;

/// <summary>Result of <see cref="GetMachineGuardingQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching MachineGuarding items.</param>
public sealed record MachineGuardingQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<MachineGuardingItemDto> Items);
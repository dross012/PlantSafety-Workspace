// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetMachineGuardingAndLotoQuery.
// SchemaVersion: 1

using PlantSafety.Application.Checklists.ChecklistTemplate.MachineGuardingAndLoto.Models;

namespace PlantSafety.Application.Checklists.ChecklistTemplate.MachineGuardingAndLoto.Queries;

/// <summary>Result of <see cref="GetMachineGuardingAndLotoQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching MachineGuardingAndLoto items.</param>
public sealed record MachineGuardingAndLotoQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<MachineGuardingAndLotoItemDto> Items);
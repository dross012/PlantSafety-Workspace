// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetMachineGuardingInspectionsQuery.
// SchemaVersion: 1

using PlantSafety.Application.SafetyProcedures.MachineGuardingInspections.Models;

namespace PlantSafety.Application.SafetyProcedures.MachineGuardingInspections.Queries;

/// <summary>Result of <see cref="GetMachineGuardingInspectionsQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching MachineGuardingInspections items.</param>
public sealed record MachineGuardingInspectionsQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<MachineGuardingInspectionsItemDto> Items);
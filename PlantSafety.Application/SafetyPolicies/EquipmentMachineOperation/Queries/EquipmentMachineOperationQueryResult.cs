// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetEquipmentMachineOperationQuery.
// SchemaVersion: 1

using PlantSafety.Application.SafetyPolicies.EquipmentMachineOperation.Models;

namespace PlantSafety.Application.SafetyPolicies.EquipmentMachineOperation.Queries;

/// <summary>Result of <see cref="GetEquipmentMachineOperationQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Equipment &amp; Machine Operation Policies items.</param>
public sealed record EquipmentMachineOperationQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<EquipmentMachineOperationItemDto> Items);
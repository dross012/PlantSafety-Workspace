// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetMachineOperationalGuardingQuery.
// SchemaVersion: 1

using PlantSafety.Application.RiskManagement.MachineOperationalGuarding.Models;

namespace PlantSafety.Application.RiskManagement.MachineOperationalGuarding.Queries;

/// <summary>Result of <see cref="GetMachineOperationalGuardingQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Machine & Operational Guarding items.</param>
public sealed record MachineOperationalGuardingQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<MachineOperationalGuardingItemDto> Items);
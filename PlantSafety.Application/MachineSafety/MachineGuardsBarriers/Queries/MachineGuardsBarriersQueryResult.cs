// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetMachineGuardsBarriersQuery.
// SchemaVersion: 1

using PlantSafety.Application.MachineSafety.MachineGuardsBarriers.Models;

namespace PlantSafety.Application.MachineSafety.MachineGuardsBarriers.Queries;

/// <summary>Result of <see cref="GetMachineGuardsBarriersQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching MachineGuardsBarriers items.</param>
public sealed record MachineGuardsBarriersQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<MachineGuardsBarriersItemDto> Items);
// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetMachinesQuery.
// SchemaVersion: 1

using PlantSafety.Application.CrossDomain.Machines.Models;

namespace PlantSafety.Application.CrossDomain.Machines.Queries;

/// <summary>Result of <see cref="GetMachinesQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Machines items.</param>
public sealed record MachinesQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<MachinesItemDto> Items);

// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetPresenceSensingDevicesQuery.
// SchemaVersion: 1

using PlantSafety.Application.MachineSafety.PresenceSensingDevices.Models;

namespace PlantSafety.Application.MachineSafety.PresenceSensingDevices.Queries;

/// <summary>Result of <see cref="GetPresenceSensingDevicesQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching PresenceSensingDevices items.</param>
public sealed record PresenceSensingDevicesQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<PresenceSensingDevicesItemDto> Items);
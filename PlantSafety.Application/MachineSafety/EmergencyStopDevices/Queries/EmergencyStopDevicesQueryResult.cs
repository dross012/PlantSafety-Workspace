// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetEmergencyStopDevicesQuery.
// SchemaVersion: 1

using PlantSafety.Application.MachineSafety.EmergencyStopDevices.Models;

namespace PlantSafety.Application.MachineSafety.EmergencyStopDevices.Queries;

/// <summary>Result of <see cref="GetEmergencyStopDevicesQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching EmergencyStopDevices items.</param>
public sealed record EmergencyStopDevicesQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<EmergencyStopDevicesItemDto> Items);
// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Result payload for ListActiveGeofenceViolationsQuery.
// SchemaVersion: 1

using PlantSafety.Application.AutonomousVehicleSafety.OperatingGeofence.Models;

namespace PlantSafety.Application.AutonomousVehicleSafety.OperatingGeofence.Queries;

/// <summary>Result of <see cref="ListActiveGeofenceViolationsQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching OperatingGeofence violation items.</param>
public sealed record OperatingGeofenceQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<OperatingGeofenceItemDto> Items);

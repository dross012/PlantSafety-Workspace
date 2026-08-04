// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Result payload for GetIncidentHistoryByAssetQuery.
// SchemaVersion: 1

using PlantSafety.Application.AutonomousVehicleSafety.AutonomousVehicleIncident.Models;

namespace PlantSafety.Application.AutonomousVehicleSafety.AutonomousVehicleIncident.Queries;

/// <summary>Result of <see cref="GetIncidentHistoryByAssetQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="AutonomousVehicleAssetId">Asset the incident history belongs to.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching AutonomousVehicleIncident items, most recent first.</param>
public sealed record AutonomousVehicleIncidentQueryResult(
    Guid TenantId,
    Guid AutonomousVehicleAssetId,
    DateTime GeneratedAt,
    IReadOnlyList<AutonomousVehicleIncidentItemDto> Items);

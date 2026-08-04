// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Shared result payload for OperatingGeofence lifecycle commands ("AutonomousVehicleSafety.OperatingGeofence").
// SchemaVersion: 1

namespace PlantSafety.Application.AutonomousVehicleSafety.Commands;

/// <summary>Result of a successful OperatingGeofence lifecycle command execution.</summary>
/// <param name="OperatingGeofenceId">Identity of the affected OperatingGeofence aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record OperatingGeofenceActionResult(
    Guid OperatingGeofenceId,
    string Status);

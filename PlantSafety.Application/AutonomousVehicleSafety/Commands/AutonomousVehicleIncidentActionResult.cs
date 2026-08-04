// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Shared result payload for AutonomousVehicleIncident commands ("AutonomousVehicleSafety.AutonomousVehicleIncident").
// SchemaVersion: 1

namespace PlantSafety.Application.AutonomousVehicleSafety.Commands;

/// <summary>Result of a successful AutonomousVehicleIncident command execution.</summary>
/// <param name="AutonomousVehicleIncidentId">Identity of the affected AutonomousVehicleIncident aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record AutonomousVehicleIncidentActionResult(
    Guid AutonomousVehicleIncidentId,
    string Status);

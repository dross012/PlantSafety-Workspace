// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Shared result payload for EmergencyStopDevices lifecycle commands ("MachineSafety.EmergencyStopDevices").
// SchemaVersion: 1

namespace PlantSafety.Application.MachineSafety.Commands;

/// <summary>Result of a successful EmergencyStopDevices lifecycle command execution.</summary>
/// <param name="EmergencyStopDevicesId">Identity of the affected EmergencyStopDevices aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record EmergencyStopDevicesActionResult(
    Guid EmergencyStopDevicesId,
    string Status);
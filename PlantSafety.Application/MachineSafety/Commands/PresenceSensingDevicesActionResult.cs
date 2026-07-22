// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Shared result payload for PresenceSensingDevices lifecycle commands ("MachineSafety.PresenceSensingDevices").
// SchemaVersion: 1

namespace PlantSafety.Application.MachineSafety.Commands;

/// <summary>Result of a successful PresenceSensingDevices lifecycle command execution.</summary>
/// <param name="PresenceSensingDevicesId">Identity of the affected PresenceSensingDevices aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record PresenceSensingDevicesActionResult(
    Guid PresenceSensingDevicesId,
    string Status);
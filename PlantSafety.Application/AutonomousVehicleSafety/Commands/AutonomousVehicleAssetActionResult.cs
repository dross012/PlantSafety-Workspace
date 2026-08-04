// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Shared result payload for AutonomousVehicleAsset lifecycle commands ("AutonomousVehicleSafety.AutonomousVehicleAsset").
// SchemaVersion: 1

namespace PlantSafety.Application.AutonomousVehicleSafety.Commands;

/// <summary>Result of a successful AutonomousVehicleAsset lifecycle command execution.</summary>
/// <param name="AutonomousVehicleAssetId">Identity of the affected AutonomousVehicleAsset aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record AutonomousVehicleAssetActionResult(
    Guid AutonomousVehicleAssetId,
    string Status);

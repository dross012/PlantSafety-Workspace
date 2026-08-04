// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Shared result payload for DischargePoint lifecycle commands ("EnvironmentalHealth.WaterDischargeCompliance.DischargePoint").
// SchemaVersion: 1

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.Commands;

/// <summary>Result of a successful DischargePoint lifecycle command execution.</summary>
/// <param name="DischargePointId">Identity of the affected DischargePoint aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record DischargePointActionResult(
    Guid DischargePointId,
    string Status);
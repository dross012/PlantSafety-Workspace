// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Shared result payload for DischargePermit lifecycle commands ("EnvironmentalHealth.WaterDischargeCompliance.DischargePermit").
// SchemaVersion: 1

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.Commands;

/// <summary>Result of a successful DischargePermit lifecycle command execution.</summary>
/// <param name="DischargePermitId">Identity of the affected DischargePermit aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record DischargePermitActionResult(
    Guid DischargePermitId,
    string Status);
// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Shared result payload for DischargeLimit lifecycle commands ("EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit").
// SchemaVersion: 1

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.Commands;

/// <summary>Result of a successful DischargeLimit lifecycle command execution.</summary>
/// <param name="DischargeLimitId">Identity of the affected DischargeLimit aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record DischargeLimitActionResult(
    Guid DischargeLimitId,
    string Status);
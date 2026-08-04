// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Shared result payload for EffluentSample lifecycle commands ("EnvironmentalHealth.WaterDischargeCompliance.EffluentSample").
// SchemaVersion: 1

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.Commands;

/// <summary>Result of a successful EffluentSample lifecycle command execution.</summary>
/// <param name="EffluentSampleId">Identity of the affected EffluentSample aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record EffluentSampleActionResult(
    Guid EffluentSampleId,
    string Status);
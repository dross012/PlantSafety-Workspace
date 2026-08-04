// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Shared result payload for LimitExceedance lifecycle commands ("EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance").
// SchemaVersion: 1

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.Commands;

/// <summary>Result of a successful LimitExceedance lifecycle command execution.</summary>
/// <param name="LimitExceedanceId">Identity of the affected LimitExceedance aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record LimitExceedanceActionResult(
    Guid LimitExceedanceId,
    string Status);
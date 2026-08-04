// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Shared result payload for SafetyCaseValidation lifecycle commands ("AutonomousVehicleSafety.SafetyCaseValidation").
// SchemaVersion: 1

namespace PlantSafety.Application.AutonomousVehicleSafety.Commands;

/// <summary>Result of a successful SafetyCaseValidation lifecycle command execution.</summary>
/// <param name="SafetyCaseValidationId">Identity of the affected SafetyCaseValidation aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
/// <param name="ExpiresAtUtc">UTC timestamp the validation expires, if validated.</param>
public sealed record SafetyCaseValidationActionResult(
    Guid SafetyCaseValidationId,
    string Status,
    DateTime? ExpiresAtUtc);

// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Shared result payload for EmergencyPreparedness lifecycle commands ("EmployeeSafety.EmergencyPreparedness").
// SchemaVersion: 1

namespace PlantSafety.Application.EmployeeSafety.Commands;

/// <summary>Result of a successful EmergencyPreparedness lifecycle command execution.</summary>
/// <param name="EmergencyPreparednessId">Identity of the affected EmergencyPreparedness aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record EmergencyPreparednessActionResult(
    Guid EmergencyPreparednessId,
    string Status);
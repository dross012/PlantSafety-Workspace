// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Shared result payload for EmergencyResponsePlanning lifecycle commands ("SafetyProcedures.EmergencyResponsePlanning").
// SchemaVersion: 1

namespace PlantSafety.Application.SafetyProcedures.Commands;

/// <summary>Result of a successful EmergencyResponsePlanning lifecycle command execution.</summary>
/// <param name="EmergencyResponsePlanningId">Identity of the affected EmergencyResponsePlanning aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record EmergencyResponsePlanningActionResult(
    Guid EmergencyResponsePlanningId,
    string Status);
// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Shared result payload for AutomatedIncidentDetection lifecycle commands
// (notify supervisor, acknowledge, escalate to incident, dismiss).
// SchemaVersion: 1

namespace PlantSafety.Application.Observability.Commands;

/// <summary>Result of a successful AutomatedIncidentDetection lifecycle command execution.</summary>
/// <param name="AutomatedIncidentDetectionId">Identity of the affected AutomatedIncidentDetection aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record AutomatedIncidentDetectionActionResult(
    Guid AutomatedIncidentDetectionId,
    string Status);

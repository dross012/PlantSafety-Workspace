// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Shared result payload for HazardCommunicationTraining lifecycle commands ("EmployeeSafety.HazardCommunicationTraining").
// SchemaVersion: 1

namespace PlantSafety.Application.EmployeeSafety.Commands;

/// <summary>Result of a successful HazardCommunicationTraining lifecycle command execution.</summary>
/// <param name="HazardCommunicationTrainingId">Identity of the affected HazardCommunicationTraining aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record HazardCommunicationTrainingActionResult(
    Guid HazardCommunicationTrainingId,
    string Status);
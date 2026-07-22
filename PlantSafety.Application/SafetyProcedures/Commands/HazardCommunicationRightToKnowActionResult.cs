// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Shared result payload for HazardCommunicationRightToKnow lifecycle commands ("SafetyProcedures.HazardCommunicationRightToKnow").
// SchemaVersion: 1

namespace PlantSafety.Application.SafetyProcedures.Commands;

/// <summary>Result of a successful HazardCommunicationRightToKnow lifecycle command execution.</summary>
/// <param name="HazardCommunicationRightToKnowId">Identity of the affected HazardCommunicationRightToKnow aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record HazardCommunicationRightToKnowActionResult(
    Guid HazardCommunicationRightToKnowId,
    string Status);
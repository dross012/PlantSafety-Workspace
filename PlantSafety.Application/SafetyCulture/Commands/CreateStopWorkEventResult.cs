// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for CreateStopWorkEventCommand.
// SchemaVersion: 1

namespace PlantSafety.Application.SafetyCulture.Commands;

/// <summary>Result of a successful <see cref="CreateStopWorkEventCommand"/> execution.</summary>
/// <param name="StopWorkEventId">Identity of the newly created StopWorkEvent aggregate.</param>
/// <param name="Status">Lifecycle status of the newly created record.</param>
public sealed record CreateStopWorkEventResult(
    Guid StopWorkEventId,
    string Status);
// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Result payload for InvestigateStopWorkEventCommand.
// SchemaVersion: 1

namespace PlantSafety.Application.SafetyCulture.Commands;

/// <summary>Result of a successful <see cref="InvestigateStopWorkEventCommand"/> execution.</summary>
/// <param name="StopWorkEventId">Identity of the investigated StopWorkEvent.</param>
/// <param name="Status">Lifecycle status after the investigation is recorded.</param>
public sealed record InvestigateStopWorkEventResult(
    Guid StopWorkEventId,
    string Status);

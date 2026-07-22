// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Result payload for ResolveStopWorkEventCommand.
// SchemaVersion: 1

namespace PlantSafety.Application.SafetyCulture.Commands;

/// <summary>Result of a successful <see cref="ResolveStopWorkEventCommand"/> execution.</summary>
/// <param name="StopWorkEventId">Identity of the resolved StopWorkEvent.</param>
/// <param name="Status">Lifecycle status after resolution.</param>
/// <param name="WasCelebrated">Whether the initiating employee was formally recognized.</param>
public sealed record ResolveStopWorkEventResult(
    Guid StopWorkEventId,
    string Status,
    bool WasCelebrated);

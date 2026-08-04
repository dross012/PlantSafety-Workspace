// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Shared result payload for WasteStream commands ("HazardousWasteManagement.WasteStreams").
// SchemaVersion: 1

namespace PlantSafety.Application.HazardousWasteManagement.Commands;

/// <summary>Result of a successful WasteStream command execution.</summary>
/// <param name="WasteStreamId">Identity of the affected WasteStream aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record WasteStreamActionResult(
    Guid WasteStreamId,
    string Status);

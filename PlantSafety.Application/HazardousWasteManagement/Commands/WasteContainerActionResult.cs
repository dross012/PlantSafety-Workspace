// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Shared result payload for WasteContainer commands ("HazardousWasteManagement.WasteContainers").
// SchemaVersion: 1

namespace PlantSafety.Application.HazardousWasteManagement.Commands;

/// <summary>Result of a successful WasteContainer command execution.</summary>
/// <param name="WasteContainerId">Identity of the affected WasteContainer aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record WasteContainerActionResult(
    Guid WasteContainerId,
    string Status);

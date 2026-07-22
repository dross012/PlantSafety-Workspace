// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Shared result payload for TwoHandControls lifecycle commands ("MachineSafety.TwoHandControls").
// SchemaVersion: 1

namespace PlantSafety.Application.MachineSafety.Commands;

/// <summary>Result of a successful TwoHandControls lifecycle command execution.</summary>
/// <param name="TwoHandControlsId">Identity of the affected TwoHandControls aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record TwoHandControlsActionResult(
    Guid TwoHandControlsId,
    string Status);
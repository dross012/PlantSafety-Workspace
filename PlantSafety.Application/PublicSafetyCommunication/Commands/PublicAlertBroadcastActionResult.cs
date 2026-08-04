// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Shared result payload for PublicAlertBroadcast commands.
// SchemaVersion: 1

namespace PlantSafety.Application.PublicSafetyCommunication.Commands;

/// <summary>Result of a successful PublicAlertBroadcast command execution.</summary>
/// <param name="PublicAlertBroadcastId">Identity of the affected PublicAlertBroadcast aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record PublicAlertBroadcastActionResult(
    Guid PublicAlertBroadcastId,
    string Status);

// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Shared result payload for IncidentNotification dispatch commands (dispatch,
// acknowledge, close).
// SchemaVersion: 1

namespace PlantSafety.Application.PublicSafetyCommunication.Commands;

/// <summary>Result of a successful IncidentNotification command execution.</summary>
/// <param name="IncidentNotificationId">Identity of the affected IncidentNotification aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record IncidentNotificationActionResult(
    Guid IncidentNotificationId,
    string Status);

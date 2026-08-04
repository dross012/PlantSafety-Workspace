// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Shared result payload for CommunityNotification commands ("CommunitySafety.CommunityNotification").
// SchemaVersion: 1

namespace PlantSafety.Application.CommunitySafety.Commands;

/// <summary>Result of a successful CommunityNotification command execution.</summary>
/// <param name="CommunityNotificationId">Identity of the affected CommunityNotification aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record CommunityNotificationActionResult(
    Guid CommunityNotificationId,
    string Status);

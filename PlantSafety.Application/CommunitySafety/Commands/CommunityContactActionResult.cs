// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Shared result payload for CommunityContact commands ("CommunitySafety.CommunityContact").
// SchemaVersion: 1

namespace PlantSafety.Application.CommunitySafety.Commands;

/// <summary>Result of a successful CommunityContact command execution.</summary>
/// <param name="CommunityContactId">Identity of the affected CommunityContact aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record CommunityContactActionResult(
    Guid CommunityContactId,
    string Status);

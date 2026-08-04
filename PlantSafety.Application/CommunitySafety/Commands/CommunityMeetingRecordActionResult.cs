// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Shared result payload for CommunityMeetingRecord commands ("CommunitySafety.CommunityMeetingRecord").
// SchemaVersion: 1

namespace PlantSafety.Application.CommunitySafety.Commands;

/// <summary>Result of a successful CommunityMeetingRecord command execution.</summary>
/// <param name="CommunityMeetingRecordId">Identity of the affected CommunityMeetingRecord aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record CommunityMeetingRecordActionResult(
    Guid CommunityMeetingRecordId,
    string Status);

// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-model item DTO for a single CommunityMeetingRecord.
// SchemaVersion: 1

namespace PlantSafety.Application.CommunitySafety.CommunityMeetingRecord.Models;

/// <summary>A single CommunityMeetingRecord (scheduled community meeting cadence history).</summary>
public sealed record CommunityMeetingRecordItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid CommunityMeetingRecordId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier.</summary>
    public required Guid SiteId { get; init; }

    /// <summary>Meeting title.</summary>
    public required string MeetingTitle { get; init; }

    /// <summary>UTC date/time the meeting was held.</summary>
    public required DateTime MeetingDateUtc { get; init; }

    /// <summary>Meeting location or virtual-meeting label.</summary>
    public required string Location { get; init; }

    /// <summary>Number of community attendees.</summary>
    public required int AttendeeCount { get; init; }

    /// <summary>Summary of topics discussed.</summary>
    public required string TopicsSummary { get; init; }

    /// <summary>Recorded action items/follow-ups, if any.</summary>
    public string? ActionItems { get; init; }

    /// <summary>Overall lifecycle status.</summary>
    public required string RecordStatus { get; init; }

    /// <summary>UTC timestamp when this record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }
}

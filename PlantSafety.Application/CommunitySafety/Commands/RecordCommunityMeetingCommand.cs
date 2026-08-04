// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Command to record a held CommunityMeetingRecord ("CommunitySafety.CommunityMeetingRecord").
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.CommunitySafety.Commands;

/// <summary>Records that a community meeting was held (scheduled community meeting cadence).</summary>
public sealed record RecordCommunityMeetingCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid SiteId,
    string MeetingTitle,
    DateTime MeetingDateUtc,
    string Location,
    int AttendeeCount,
    string TopicsSummary,
    string? ActionItems,
    Guid RecordedByUserId,
    Guid PerformedByUserId) : ICommand<Result<CommunityMeetingRecordActionResult>>
{
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    Guid? ICommand<Result<CommunityMeetingRecordActionResult>>.CausationId => CausationId;

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => PerformedByUserId;
}

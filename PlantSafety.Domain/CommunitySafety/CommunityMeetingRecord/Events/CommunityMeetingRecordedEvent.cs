// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityMeetingRecord
//  Layer:          Domain
//  File:           CommunityMeetingRecordedEvent.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Raised when a new CommunityMeetingRecord aggregate is recorded. Additive-only
//                  evolution.
// ============================================================================
using PlantSafety.Domain.CommunitySafety.CommunityMeetingRecord.ValueObjects.Ids;

namespace PlantSafety.Domain.CommunitySafety.CommunityMeetingRecord.Events;

/// <summary>Raised when a new <see cref="Aggregates.CommunityMeetingRecord"/> aggregate is recorded.</summary>
public sealed record CommunityMeetingRecordedEvent(
    CommunityMeetingRecordEventId EventId,
    CommunityMeetingRecordId      CommunityMeetingRecordId,
    string                         MeetingTitle,
    UtcTimestamp                   MeetingDateUtc,
    UtcTimestamp                   RecordedAtUtc,
    PerformedByActor                Actor,
    CorrelationId                   CorrelationId,
    CausationId                     CausationId
) : ICommunityMeetingRecordDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "CommunityMeetingRecorded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => RecordedAtUtc;
}

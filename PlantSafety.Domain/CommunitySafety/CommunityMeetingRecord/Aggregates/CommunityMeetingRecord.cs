// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityMeetingRecord
//  Layer:          Domain
//  File:           CommunityMeetingRecord.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the CommunityMeetingRecord bounded context (Outreach &amp;
//                  Notification History subdomain of CommunitySafety). Records that a community
//                  meeting (e.g. a Community Advisory Panel session) was held, its topics,
//                  attendee count, and action items. Scheduling hook: the periodic meeting cadence
//                  itself is driven by a Scheduling-module background job that issues
//                  RecordCommunityMeeting commands on cadence; this aggregate is the append-only
//                  record of what was actually held, not the scheduling calendar. Additive-only
//                  evolution.
// ============================================================================
using PlantSafety.Domain.CommunitySafety.CommunityMeetingRecord.Enums;
using PlantSafety.Domain.CommunitySafety.CommunityMeetingRecord.Events;
using PlantSafety.Domain.CommunitySafety.CommunityMeetingRecord.Exceptions;
using PlantSafety.Domain.CommunitySafety.CommunityMeetingRecord.Rules;
using PlantSafety.Domain.CommunitySafety.CommunityMeetingRecord.ValueObjects.Ids;

namespace PlantSafety.Domain.CommunitySafety.CommunityMeetingRecord.Aggregates;

/// <summary>Aggregate root for the CommunityMeetingRecord bounded context.</summary>
public sealed partial class CommunityMeetingRecord
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Strong-typed identity of this CommunityMeetingRecord aggregate.</summary>
    public CommunityMeetingRecordId CommunityMeetingRecordId { get; private set; }

    /// <summary>Tenant that owns this meeting record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site the meeting relates to.</summary>
    public Guid SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public CommunityMeetingRecordStatus Status { get; private set; }

    // ---- Meeting-specific business properties ----------------------------------

    /// <summary>Meeting title (e.g. "Q3 Community Advisory Panel").</summary>
    public string MeetingTitle { get; private set; } = string.Empty;

    /// <summary>UTC date/time the meeting was held.</summary>
    public UtcTimestamp MeetingDateUtc { get; private set; }

    /// <summary>Meeting location or virtual-meeting label.</summary>
    public string Location { get; private set; } = string.Empty;

    /// <summary>Number of community attendees.</summary>
    public int AttendeeCount { get; private set; }

    /// <summary>Summary of topics discussed.</summary>
    public string TopicsSummary { get; private set; } = string.Empty;

    /// <summary>Recorded action items/follow-ups from the meeting, if any.</summary>
    public string? ActionItems { get; private set; }

    /// <summary>Identity of the user who recorded the meeting.</summary>
    public UserId RecordedByUserId { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events ---------------------------------------------------------

    private readonly List<ICommunityMeetingRecordDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<ICommunityMeetingRecordDomainEvent> DomainEvents
        => new ReadOnlyCollection<ICommunityMeetingRecordDomainEvent>(_domainEvents);

    // For rehydration only.
    private CommunityMeetingRecord() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>
    /// Records a new CommunityMeetingRecord aggregate and raises
    /// <see cref="CommunityMeetingRecordedEvent"/>.
    /// </summary>
    /// <exception cref="ArgumentException">A required field is missing or empty.</exception>
    public static CommunityMeetingRecord Record(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid siteId,
        string meetingTitle,
        UtcTimestamp meetingDateUtc,
        string location,
        int attendeeCount,
        string topicsSummary,
        UserId recordedByUserId,
        string? actionItems = null)
    {
        var normalizedTitle = (meetingTitle ?? string.Empty).Trim();
        var normalizedLocation = (location ?? string.Empty).Trim();
        var normalizedTopics = (topicsSummary ?? string.Empty).Trim();

        if (tenantId == Guid.Empty)
            throw new ArgumentException("TenantId is required.", nameof(tenantId));
        if (siteId == Guid.Empty)
            throw new ArgumentException("SiteId is required.", nameof(siteId));
        if (string.IsNullOrWhiteSpace(normalizedTitle))
            throw new ArgumentException("MeetingTitle is required.", nameof(meetingTitle));
        if (string.IsNullOrWhiteSpace(normalizedLocation))
            throw new ArgumentException("Location is required.", nameof(location));
        if (string.IsNullOrWhiteSpace(normalizedTopics))
            throw new ArgumentException("TopicsSummary is required.", nameof(topicsSummary));
        if (attendeeCount < 0)
            throw new ArgumentException("AttendeeCount cannot be negative.", nameof(attendeeCount));

        var agg = new CommunityMeetingRecord
        {
            CommunityMeetingRecordId = CommunityMeetingRecordId.New(),
            TenantId = tenantId,
            SiteId = siteId,
            Status = CommunityMeetingRecordStatus.Recorded,
            MeetingTitle = normalizedTitle,
            MeetingDateUtc = meetingDateUtc,
            Location = normalizedLocation,
            AttendeeCount = attendeeCount,
            TopicsSummary = normalizedTopics,
            ActionItems = string.IsNullOrWhiteSpace(actionItems) ? null : actionItems.Trim(),
            RecordedByUserId = recordedByUserId,
            CreatedAtUtc = createdAtUtc,
            CreatedBy = createdBy,
        };

        agg.AddDomainEvent(new CommunityMeetingRecordedEvent(
            CommunityMeetingRecordEventId.New(), agg.CommunityMeetingRecordId, agg.MeetingTitle,
            agg.MeetingDateUtc, createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    /// <summary>Reconstructs a CommunityMeetingRecord aggregate from previously persisted state, without raising domain events.</summary>
    public static CommunityMeetingRecord Rehydrate(
        CommunityMeetingRecordId communityMeetingRecordId,
        Guid tenantId,
        Guid siteId,
        CommunityMeetingRecordStatus status,
        string meetingTitle,
        UtcTimestamp meetingDateUtc,
        string location,
        int attendeeCount,
        string topicsSummary,
        string? actionItems,
        UserId recordedByUserId,
        UtcTimestamp createdAtUtc,
        UserId createdBy)
        => new()
        {
            CommunityMeetingRecordId = communityMeetingRecordId,
            TenantId = tenantId,
            SiteId = siteId,
            Status = status,
            MeetingTitle = meetingTitle,
            MeetingDateUtc = meetingDateUtc,
            Location = location,
            AttendeeCount = attendeeCount,
            TopicsSummary = topicsSummary,
            ActionItems = actionItems,
            RecordedByUserId = recordedByUserId,
            CreatedAtUtc = createdAtUtc,
            CreatedBy = createdBy,
        };

    // ---- Lifecycle: Archive ---------------------------------------------------

    /// <summary>Archives the meeting record for long-term retention.</summary>
    /// <exception cref="CommunityMeetingRecordInvalidStatusTransitionException">The transition is not permitted.</exception>
    public void Archive(UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (!CommunityMeetingRecordLifecycleRules.CanTransition(Status, CommunityMeetingRecordStatus.Archived))
            throw new CommunityMeetingRecordInvalidStatusTransitionException(
                CommunityMeetingRecordId, Status.ToString(), CommunityMeetingRecordStatus.Archived.ToString(), correlationId, causationId);

        Status = CommunityMeetingRecordStatus.Archived;
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(ICommunityMeetingRecordDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}

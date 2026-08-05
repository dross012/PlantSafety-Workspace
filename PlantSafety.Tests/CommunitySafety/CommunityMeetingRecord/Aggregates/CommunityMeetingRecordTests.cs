// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.CommunitySafety.CommunityMeetingRecord.Aggregates
//  Layer:          Tests (Unit)
//  File:           CommunityMeetingRecordTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Lifecycle/invariant tests for the CommunityMeetingRecord aggregate root. Covers
//                  Record invariants and the Recorded/Archived lifecycle. Isolated, deterministic,
//                  <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.CommunitySafety.CommunityMeetingRecord.Enums;
using PlantSafety.Domain.CommunitySafety.CommunityMeetingRecord.Exceptions;
using Agg = PlantSafety.Domain.CommunitySafety.CommunityMeetingRecord.Aggregates.CommunityMeetingRecord;

namespace PlantSafety.Tests.CommunitySafety.CommunityMeetingRecord.Aggregates;

public sealed class CommunityMeetingRecordTests : UnitTestBase
{
    private static Agg RecordedMeeting(int attendeeCount = 12) =>
        Agg.Record(
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            siteId: Guid.NewGuid(),
            meetingTitle: "Q3 Community Advisory Panel",
            meetingDateUtc: FixedTimestamp,
            location: "Community Center, Room B",
            attendeeCount: attendeeCount,
            topicsSummary: "Reviewed Q3 flare activity and upcoming maintenance schedule.",
            recordedByUserId: TestUserId,
            actionItems: "Publish maintenance schedule to community newsletter.");

    [Fact]
    public void Record_WithValidFields_CreatesRecordedMeetingAndRaisesEvent()
    {
        var meeting = RecordedMeeting();

        meeting.Status.Should().Be(CommunityMeetingRecordStatus.Recorded);
        meeting.DomainEvents.Should().ContainSingle();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Record_WithMissingMeetingTitle_ThrowsArgumentException(string? title)
    {
        var act = () => Agg.Record(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            TestTenantId.Value, Guid.NewGuid(), title!, FixedTimestamp, "Location", 5, "Topics", TestUserId);

        act.Should().Throw<ArgumentException>().WithParameterName("meetingTitle");
    }

    [Fact]
    public void Record_WithMissingLocation_ThrowsArgumentException()
    {
        var act = () => Agg.Record(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            TestTenantId.Value, Guid.NewGuid(), "Title", FixedTimestamp, "", 5, "Topics", TestUserId);

        act.Should().Throw<ArgumentException>().WithParameterName("location");
    }

    [Fact]
    public void Record_WithMissingTopicsSummary_ThrowsArgumentException()
    {
        var act = () => Agg.Record(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            TestTenantId.Value, Guid.NewGuid(), "Title", FixedTimestamp, "Location", 5, "   ", TestUserId);

        act.Should().Throw<ArgumentException>().WithParameterName("topicsSummary");
    }

    [Fact]
    public void Record_WithNegativeAttendeeCount_ThrowsArgumentException()
    {
        var act = () => Agg.Record(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            TestTenantId.Value, Guid.NewGuid(), "Title", FixedTimestamp, "Location", -1, "Topics", TestUserId);

        act.Should().Throw<ArgumentException>().WithParameterName("attendeeCount");
    }

    [Fact]
    public void Record_WithZeroAttendeeCount_Succeeds()
    {
        var meeting = RecordedMeeting(attendeeCount: 0);

        meeting.AttendeeCount.Should().Be(0);
    }

    [Fact]
    public void Archive_FromRecorded_Transitions()
    {
        var meeting = RecordedMeeting();

        meeting.Archive(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        meeting.Status.Should().Be(CommunityMeetingRecordStatus.Archived);
    }

    [Fact]
    public void Archive_WhenAlreadyArchived_ThrowsInvalidStatusTransitionException()
    {
        var meeting = RecordedMeeting();
        meeting.Archive(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => meeting.Archive(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<CommunityMeetingRecordInvalidStatusTransitionException>();
    }

    [Fact]
    public void Rehydrate_ReconstructsStateWithoutRaisingDomainEvents()
    {
        var original = RecordedMeeting();

        var rehydrated = Agg.Rehydrate(
            original.CommunityMeetingRecordId, original.TenantId, original.SiteId, original.Status,
            original.MeetingTitle, original.MeetingDateUtc, original.Location, original.AttendeeCount,
            original.TopicsSummary, original.ActionItems, original.RecordedByUserId, original.CreatedAtUtc,
            original.CreatedBy);

        rehydrated.CommunityMeetingRecordId.Should().Be(original.CommunityMeetingRecordId);
        rehydrated.DomainEvents.Should().BeEmpty("rehydration must never re-raise domain events");
    }
}

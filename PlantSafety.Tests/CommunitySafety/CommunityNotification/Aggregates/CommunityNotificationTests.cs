// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.CommunitySafety.CommunityNotification.Aggregates
//  Layer:          Tests (Unit)
//  File:           CommunityNotificationTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Lifecycle/invariant tests for the CommunityNotification aggregate root
//                  (Outreach &amp; Notification History subdomain). Covers Send invariants and the
//                  Sent/Acknowledged/Failed/Archived lifecycle. Isolated, deterministic, <50ms per
//                  test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.CommunitySafety.CommunityNotification.Enums;
using PlantSafety.Domain.CommunitySafety.CommunityNotification.Exceptions;
using Agg = PlantSafety.Domain.CommunitySafety.CommunityNotification.Aggregates.CommunityNotification;

namespace PlantSafety.Tests.CommunitySafety.CommunityNotification.Aggregates;

public sealed class CommunityNotificationTests : UnitTestBase
{
    private static Agg SentNotification() =>
        Agg.Send(
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            siteId: Guid.NewGuid(),
            channel: CommunityNotificationChannel.Email,
            recipientContactId: Guid.NewGuid(),
            subject: "Planned maintenance notice",
            messageBody: "Elevated flare activity expected Tuesday 2-4am.");

    [Fact]
    public void Send_WithValidFields_CreatesSentNotificationAndRaisesEvent()
    {
        var notification = SentNotification();

        notification.Status.Should().Be(CommunityNotificationStatus.Sent);
        notification.DomainEvents.Should().ContainSingle();
    }

    [Fact]
    public void Send_WithEmptyRecipientContactId_ThrowsArgumentException()
    {
        var act = () => Agg.Send(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            TestTenantId.Value, null, CommunityNotificationChannel.Sms, Guid.Empty, "Subj", "Body");

        act.Should().Throw<ArgumentException>().WithParameterName("recipientContactId");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Send_WithMissingMessageBody_ThrowsArgumentException(string? body)
    {
        var act = () => Agg.Send(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            TestTenantId.Value, null, CommunityNotificationChannel.Sms, Guid.NewGuid(), "Subj", body!);

        act.Should().Throw<ArgumentException>().WithParameterName("messageBody");
    }

    [Fact]
    public void Acknowledge_FromSent_TransitionsAndRaisesEvent()
    {
        var notification = SentNotification();

        notification.Acknowledge(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        notification.Status.Should().Be(CommunityNotificationStatus.Acknowledged);
        notification.AcknowledgedAtUtc.Should().Be(FixedTimestamp);
        notification.DomainEvents.Should().HaveCount(2, "Send + Acknowledge each raise one domain event");
    }

    [Fact]
    public void MarkFailed_FromSent_TransitionsAndRecordsReason()
    {
        var notification = SentNotification();

        notification.MarkFailed("Bounced: mailbox unavailable.", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        notification.Status.Should().Be(CommunityNotificationStatus.Failed);
        notification.FailureReason.Should().Be("Bounced: mailbox unavailable.");
    }

    [Fact]
    public void MarkFailed_WithEmptyReason_ThrowsArgumentException()
    {
        var notification = SentNotification();

        var act = () => notification.MarkFailed("", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<ArgumentException>().WithParameterName("reason");
    }

    [Fact]
    public void Acknowledge_WhenAlreadyAcknowledged_ThrowsInvalidStatusTransitionException()
    {
        var notification = SentNotification();
        notification.Acknowledge(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => notification.Acknowledge(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<CommunityNotificationInvalidStatusTransitionException>();
    }

    [Fact]
    public void MarkFailed_WhenAlreadyFailed_ThrowsInvalidStatusTransitionException()
    {
        var notification = SentNotification();
        notification.MarkFailed("First failure", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => notification.MarkFailed("Second failure", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<CommunityNotificationInvalidStatusTransitionException>();
    }

    [Theory]
    [InlineData(CommunityNotificationStatus.Sent)]
    [InlineData(CommunityNotificationStatus.Acknowledged)]
    [InlineData(CommunityNotificationStatus.Failed)]
    public void Archive_FromAnyNonTerminalStatus_Transitions(CommunityNotificationStatus fromStatus)
    {
        var notification = SentNotification();
        switch (fromStatus)
        {
            case CommunityNotificationStatus.Acknowledged:
                notification.Acknowledge(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
                break;
            case CommunityNotificationStatus.Failed:
                notification.MarkFailed("reason", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
                break;
        }

        notification.Archive(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        notification.Status.Should().Be(CommunityNotificationStatus.Archived);
    }

    [Fact]
    public void Rehydrate_ReconstructsStateWithoutRaisingDomainEvents()
    {
        var original = SentNotification();

        var rehydrated = Agg.Rehydrate(
            original.CommunityNotificationId, original.TenantId, original.SiteId, original.Status,
            original.Channel, original.RecipientContactId, original.Subject, original.MessageBody,
            original.RelatedConcernId, original.RelatedIncidentId, original.SentAtUtc, original.AcknowledgedAtUtc,
            original.FailureReason, original.CreatedAtUtc, original.CreatedBy);

        rehydrated.CommunityNotificationId.Should().Be(original.CommunityNotificationId);
        rehydrated.DomainEvents.Should().BeEmpty("rehydration must never re-raise domain events");
    }
}

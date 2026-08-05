// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.PublicSafetyCommunication.Domain
//  Layer:          Tests (Unit)
//  File:           PublicAlertBroadcastTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Lifecycle/invariant tests for the PublicAlertBroadcast aggregate root -- Send
//                  (Create), ConfirmDelivery, RecordFailure, Cancel and their guard rules.
//                  Isolated, deterministic, <50ms per test. No I/O.
// ============================================================================
using PlantSafety.Domain.PublicSafetyCommunication.PublicAlertBroadcast.Enums;
using PlantSafety.Domain.PublicSafetyCommunication.PublicAlertBroadcast.Events;
using PlantSafety.Domain.PublicSafetyCommunication.PublicAlertBroadcast.Exceptions;
using BroadcastAggregate = PlantSafety.Domain.PublicSafetyCommunication.PublicAlertBroadcast.Aggregates.PublicAlertBroadcast;

namespace PlantSafety.Tests.PublicSafetyCommunication.Domain;

public sealed class PublicAlertBroadcastTests : UnitTestBase
{
    private static BroadcastAggregate CreateBroadcast() =>
        BroadcastAggregate.Create(
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            siteId: null,
            incidentId: Guid.NewGuid(),
            severity: PublicAlertSeverity.Warning,
            channel: PublicBroadcastChannel.Ipaws,
            headline: "Shelter in place -- chemical release",
            message: "A chemical release has occurred at the facility. Residents within 1 mile should shelter in place.",
            targetArea: "1-mile radius of Plant Road facility");

    [Fact]
    public void Create_WithValidData_InitializesSentBroadcastAndRaisesEvent()
    {
        var broadcast = CreateBroadcast();

        broadcast.Status.Should().Be(PublicAlertBroadcastStatus.Sent);
        broadcast.IssuedAtUtc.Should().Be(FixedTimestamp);
        broadcast.IssuedBy.Should().Be(TestUserId);
        broadcast.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<PublicAlertBroadcastSentEvent>();
    }

    [Fact]
    public void ConfirmDelivery_WhenSent_TransitionsToDeliveredAndRaisesEvent()
    {
        var broadcast = CreateBroadcast();
        broadcast.ClearDomainEvents();

        broadcast.ConfirmDelivery(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        broadcast.Status.Should().Be(PublicAlertBroadcastStatus.Delivered);
        broadcast.DeliveryConfirmedAtUtc.Should().Be(FixedTimestamp);
        broadcast.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<PublicAlertBroadcastDeliveryConfirmedEvent>();
    }

    [Fact]
    public void ConfirmDelivery_WhenAlreadyDelivered_ThrowsInvalidStatusTransition()
    {
        var broadcast = CreateBroadcast();
        broadcast.ConfirmDelivery(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => broadcast.ConfirmDelivery(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<PublicAlertBroadcastInvalidStatusTransitionException>();
    }

    [Fact]
    public void ConfirmDelivery_WhenCancelled_ThrowsInvalidStatusTransition()
    {
        var broadcast = CreateBroadcast();
        broadcast.Cancel(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => broadcast.ConfirmDelivery(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<PublicAlertBroadcastInvalidStatusTransitionException>();
    }

    [Fact]
    public void RecordFailure_SetsFailedStatusAndReasonAndRaisesEvent()
    {
        var broadcast = CreateBroadcast();
        broadcast.ClearDomainEvents();

        broadcast.RecordFailure("IPAWS gateway timed out.", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        broadcast.Status.Should().Be(PublicAlertBroadcastStatus.Failed);
        broadcast.FailureReason.Should().Be("IPAWS gateway timed out.");
        broadcast.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<PublicAlertBroadcastFailedEvent>();
    }

    [Fact]
    public void Cancel_WhenNotAlreadyCancelled_TransitionsToCancelledAndRaisesEvent()
    {
        var broadcast = CreateBroadcast();
        broadcast.ClearDomainEvents();

        broadcast.Cancel(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        broadcast.Status.Should().Be(PublicAlertBroadcastStatus.Cancelled);
        broadcast.CancelledAtUtc.Should().Be(FixedTimestamp);
        broadcast.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<PublicAlertBroadcastCancelledEvent>();
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_ThrowsInvalidStatusTransition()
    {
        var broadcast = CreateBroadcast();
        broadcast.Cancel(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => broadcast.Cancel(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<PublicAlertBroadcastInvalidStatusTransitionException>();
    }

    [Fact]
    public void Cancel_AfterDelivered_IsStillAllowed()
    {
        // Cancel has no EnsureStatusIsOneOf guard other than "not already Cancelled" -- a
        // delivered broadcast can still be cancelled (e.g. superseded by an all-clear).
        var broadcast = CreateBroadcast();
        broadcast.ConfirmDelivery(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => broadcast.Cancel(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().NotThrow();
        broadcast.Status.Should().Be(PublicAlertBroadcastStatus.Cancelled);
    }
}

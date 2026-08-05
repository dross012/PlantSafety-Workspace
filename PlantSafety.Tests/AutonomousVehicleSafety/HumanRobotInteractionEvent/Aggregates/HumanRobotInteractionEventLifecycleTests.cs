// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.AutonomousVehicleSafety.HumanRobotInteractionEvent
//  Layer:          Tests (Unit)
//  File:           HumanRobotInteractionEventLifecycleTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Lifecycle/invariant tests for the HumanRobotInteractionEvent aggregate root:
//                  Create (Record)/BeginReview/Close transitions and their guard rules.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.AutonomousVehicleSafety.HumanRobotInteractionEvent.Enums;
using PlantSafety.Domain.AutonomousVehicleSafety.HumanRobotInteractionEvent.Events;
using Agg = PlantSafety.Domain.AutonomousVehicleSafety.HumanRobotInteractionEvent.Aggregates.HumanRobotInteractionEvent;

namespace PlantSafety.Tests.AutonomousVehicleSafety.HumanRobotInteractionEvent.Aggregates;

public sealed class HumanRobotInteractionEventLifecycleTests : UnitTestBase
{
    private static Agg RecordEvent() => Agg.Create(
        createdBy: TestUserId,
        createdAtUtc: FixedTimestamp,
        correlationId: TestCorrelationId,
        causationId: TestCausationId,
        actor: TestActor,
        tenantId: TestTenantId.Value,
        autonomousVehicleAssetId: Guid.NewGuid(),
        interactionType: HumanRobotInteractionType.NearMiss,
        severityLevel: InteractionSeverityLevel.High,
        distanceMeters: 0.5m,
        locationDescription: "Loading dock 3");

    [Fact]
    public void Create_ShouldInitializeWithRecordedStatus_AndRaiseRecordedEvent()
    {
        var evt = RecordEvent();

        evt.Status.Should().Be(HumanRobotInteractionEventStatus.Recorded);
        evt.DistanceMeters.Should().Be(0.5m);
        evt.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<HumanRobotInteractionEventRecordedEvent>();
    }

    [Fact]
    public void BeginReview_WhenRecorded_ShouldTransitionToUnderReview()
    {
        var evt = RecordEvent();

        evt.BeginReview(TestActor, TestCorrelationId, TestCausationId);

        evt.Status.Should().Be(HumanRobotInteractionEventStatus.UnderReview);
    }

    [Fact]
    public void BeginReview_WhenClosed_ShouldThrowInvalidOperationException()
    {
        var evt = RecordEvent();
        evt.Close(TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => evt.BeginReview(TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Close_WhenRecorded_ShouldTransitionDirectlyToClosed()
    {
        var evt = RecordEvent();

        evt.Close(TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        evt.Status.Should().Be(HumanRobotInteractionEventStatus.Closed);
        evt.ReviewedByUserId.Should().Be(TestUserId);
    }

    [Fact]
    public void Close_WhenUnderReview_ShouldTransitionToClosed()
    {
        var evt = RecordEvent();
        evt.BeginReview(TestActor, TestCorrelationId, TestCausationId);

        evt.Close(TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        evt.Status.Should().Be(HumanRobotInteractionEventStatus.Closed);
    }

    [Fact]
    public void Close_WhenAlreadyClosed_ShouldThrowInvalidOperationException()
    {
        var evt = RecordEvent();
        evt.Close(TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => evt.Close(TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<InvalidOperationException>();
    }
}

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.PublicSafetyCommunication.Domain
//  Layer:          Tests (Unit)
//  File:           IncidentNotificationTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Lifecycle/invariant tests for the IncidentNotification aggregate root --
//                  Dispatch (Create), Acknowledge, Close, RecordDeliveryFailure and their guard
//                  rules. Isolated, deterministic, <50ms per test. No I/O.
// ============================================================================
using PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.ValueObjects.Ids;
using PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.Enums;
using PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.Events;
using PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.Exceptions;
using IncidentNotificationAggregate = PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.Aggregates.IncidentNotification;

namespace PlantSafety.Tests.PublicSafetyCommunication.Domain;

public sealed class IncidentNotificationTests : UnitTestBase
{
    private static IncidentNotificationAggregate CreateNotification() =>
        IncidentNotificationAggregate.Create(
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            siteId: null,
            incidentId: Guid.NewGuid(),
            publicSafetyAgencyId: PublicSafetyAgencyId.New(),
            channel: PublicSafetyDeliveryChannel.Phone,
            message: "Ammonia release detected in Unit 3 -- requesting HazMat response.");

    [Fact]
    public void Create_WithValidData_InitializesDispatchedNotificationAndRaisesEvent()
    {
        var notification = CreateNotification();

        notification.Status.Should().Be(IncidentNotificationStatus.Dispatched);
        notification.DispatchedAtUtc.Should().Be(FixedTimestamp);
        notification.DispatchedBy.Should().Be(TestUserId);
        notification.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<IncidentNotificationDispatchedEvent>();
    }

    [Fact]
    public void Acknowledge_WhenDispatched_TransitionsToAcknowledgedAndRaisesEvent()
    {
        var notification = CreateNotification();
        notification.ClearDomainEvents();

        notification.Acknowledge("County Dispatch Desk 4", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        notification.Status.Should().Be(IncidentNotificationStatus.Acknowledged);
        notification.AcknowledgedByContactName.Should().Be("County Dispatch Desk 4");
        notification.AcknowledgedAtUtc.Should().Be(FixedTimestamp);
        notification.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<IncidentNotificationAcknowledgedEvent>();
    }

    [Fact]
    public void Acknowledge_WhenAlreadyAcknowledged_ThrowsInvalidStatusTransition()
    {
        var notification = CreateNotification();
        notification.Acknowledge("Desk 4", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => notification.Acknowledge("Desk 4 again", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<IncidentNotificationInvalidStatusTransitionException>();
    }

    [Fact]
    public void Acknowledge_WhenClosed_ThrowsInvalidStatusTransition()
    {
        var notification = CreateNotification();
        notification.Close(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => notification.Acknowledge("Desk 4", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<IncidentNotificationInvalidStatusTransitionException>();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Close_WhenDispatchedOrAcknowledged_TransitionsToClosedAndRaisesEvent(bool acknowledgeFirst)
    {
        var notification = CreateNotification();
        if (acknowledgeFirst)
            notification.Acknowledge("Desk 4", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        notification.ClearDomainEvents();

        notification.Close(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        notification.Status.Should().Be(IncidentNotificationStatus.Closed);
        notification.ClosedAtUtc.Should().Be(FixedTimestamp);
        notification.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<IncidentNotificationClosedEvent>();
    }

    [Fact]
    public void Close_WhenAlreadyClosed_ThrowsInvalidStatusTransition()
    {
        var notification = CreateNotification();
        notification.Close(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => notification.Close(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<IncidentNotificationInvalidStatusTransitionException>();
    }

    [Fact]
    public void RecordDeliveryFailure_SetsFailedStatusAndReasonAndRaisesEvent()
    {
        var notification = CreateNotification();
        notification.ClearDomainEvents();

        notification.RecordDeliveryFailure("Phone line unreachable after 3 attempts.", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        notification.Status.Should().Be(IncidentNotificationStatus.Failed);
        notification.FailureReason.Should().Be("Phone line unreachable after 3 attempts.");
        notification.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<IncidentNotificationDeliveryFailedEvent>();
    }

    [Fact]
    public void RecordDeliveryFailure_IsAllowedRegardlessOfCurrentStatus_ForRiskEscalationHook()
    {
        // RecordDeliveryFailure deliberately has no EnsureStatusIsOneOf guard -- it must be
        // callable even from Acknowledged/Closed so the RiskManagement escalation hook can react
        // to a late-discovered delivery failure without being blocked by lifecycle state.
        var notification = CreateNotification();
        notification.Close(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => notification.RecordDeliveryFailure("Late failure discovered.", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().NotThrow();
        notification.Status.Should().Be(IncidentNotificationStatus.Failed);
    }
}

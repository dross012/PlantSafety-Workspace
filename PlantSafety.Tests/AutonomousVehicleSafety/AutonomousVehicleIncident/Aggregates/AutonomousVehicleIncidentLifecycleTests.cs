// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.AutonomousVehicleSafety.AutonomousVehicleIncident
//  Layer:          Tests (Unit)
//  File:           AutonomousVehicleIncidentLifecycleTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Lifecycle/invariant tests for the AutonomousVehicleIncident aggregate root:
//                  Create (Record)/BeginInvestigation/TriggerSuspensionReview/Close transitions and
//                  their guard rules. Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleIncident.Enums;
using PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleIncident.Events;
using PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleIncident.Exceptions;
using Agg = PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleIncident.Aggregates.AutonomousVehicleIncident;

namespace PlantSafety.Tests.AutonomousVehicleSafety.AutonomousVehicleIncident.Aggregates;

public sealed class AutonomousVehicleIncidentLifecycleTests : UnitTestBase
{
    private static Agg RecordIncident() => Agg.Create(
        createdBy: TestUserId,
        createdAtUtc: FixedTimestamp,
        correlationId: TestCorrelationId,
        causationId: TestCausationId,
        actor: TestActor,
        tenantId: TestTenantId.Value,
        autonomousVehicleAssetId: Guid.NewGuid(),
        incidentType: AutonomousVehicleIncidentType.GeofenceBreach,
        severityLevel: IncidentSeverityLevel.High,
        descriptionSummary: "Vehicle breached the north exclusion zone");

    [Fact]
    public void Create_ShouldInitializeWithReportedStatus_AndRaiseRecordedEvent()
    {
        var incident = RecordIncident();

        incident.Status.Should().Be(AutonomousVehicleIncidentStatus.Reported);
        incident.SuspensionTriggered.Should().BeFalse();
        incident.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<AutonomousVehicleIncidentRecordedEvent>();
    }

    [Fact]
    public void BeginInvestigation_WhenReported_ShouldTransitionToUnderInvestigation()
    {
        var incident = RecordIncident();
        var lead = UserId.FromGuid(Guid.NewGuid());

        incident.BeginInvestigation(lead, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        incident.Status.Should().Be(AutonomousVehicleIncidentStatus.UnderInvestigation);
        incident.InvestigationLeadUserId.Should().Be(lead);
    }

    [Fact]
    public void BeginInvestigation_WhenAlreadyUnderInvestigation_ShouldThrowInvalidOperationException()
    {
        var incident = RecordIncident();
        incident.BeginInvestigation(UserId.FromGuid(Guid.NewGuid()), FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => incident.BeginInvestigation(UserId.FromGuid(Guid.NewGuid()), FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void TriggerSuspensionReview_WhenReported_ShouldSetFlag_AndTransitionToCorrectiveActionInProgress_AndRaiseEvent()
    {
        var incident = RecordIncident();

        incident.TriggerSuspensionReview(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        incident.SuspensionTriggered.Should().BeTrue();
        incident.Status.Should().Be(AutonomousVehicleIncidentStatus.CorrectiveActionInProgress);
        incident.DomainEvents.Should().Contain(e => e.GetType().Name == "AutonomousVehicleIncidentSuspensionTriggeredEvent");
    }

    [Fact]
    public void TriggerSuspensionReview_WhenClosed_ShouldThrowInvalidOperationException()
    {
        var incident = RecordIncident();
        incident.Close(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => incident.TriggerSuspensionReview(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Close_WhenReported_ShouldTransitionToClosed_AndSetClosedAt()
    {
        var incident = RecordIncident();

        incident.Close(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        incident.Status.Should().Be(AutonomousVehicleIncidentStatus.Closed);
        incident.ClosedAtUtc.Should().Be(FixedTimestamp);
    }

    [Fact]
    public void Close_WhenAlreadyClosed_ShouldThrowInvalidStatusTransitionException()
    {
        var incident = RecordIncident();
        incident.Close(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => incident.Close(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<AutonomousVehicleIncidentInvalidStatusTransitionException>();
    }
}

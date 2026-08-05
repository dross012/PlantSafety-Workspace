// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.AutonomousVehicleSafety.OperatingGeofence
//  Layer:          Tests (Unit)
//  File:           OperatingGeofenceLifecycleTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Lifecycle/invariant tests for the OperatingGeofence aggregate root: Create
//                  (Define)/Activate/RecordViolation/ScheduleAudit/RecordAudit/Retire transitions
//                  and their guard rules. Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.AutonomousVehicleSafety.OperatingGeofence.Enums;
using PlantSafety.Domain.AutonomousVehicleSafety.OperatingGeofence.Events;
using PlantSafety.Domain.AutonomousVehicleSafety.OperatingGeofence.Exceptions;
using Agg = PlantSafety.Domain.AutonomousVehicleSafety.OperatingGeofence.Aggregates.OperatingGeofence;

namespace PlantSafety.Tests.AutonomousVehicleSafety.OperatingGeofence.Aggregates;

public sealed class OperatingGeofenceLifecycleTests : UnitTestBase
{
    private static Agg DefineGeofence() => Agg.Create(
        createdBy: TestUserId,
        createdAtUtc: FixedTimestamp,
        correlationId: TestCorrelationId,
        causationId: TestCausationId,
        actor: TestActor,
        tenantId: TestTenantId.Value,
        geofenceName: "North Yard Exclusion",
        zoneType: GeofenceZoneType.ExclusionZone,
        boundaryDefinition: "POLYGON((0 0,1 0,1 1,0 1,0 0))",
        maxSpeedKph: null);

    [Fact]
    public void Create_ShouldInitializeWithDraftStatus_AndRaiseDefinedEvent()
    {
        var geofence = DefineGeofence();

        geofence.Status.Should().Be(OperatingGeofenceStatus.Draft);
        geofence.ViolationCount.Should().Be(0);
        geofence.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<OperatingGeofenceDefinedEvent>();
    }

    [Fact]
    public void Activate_WhenDraft_ShouldTransitionToActive()
    {
        var geofence = DefineGeofence();

        geofence.Activate(TestActor, TestCorrelationId, TestCausationId);

        geofence.Status.Should().Be(OperatingGeofenceStatus.Active);
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ShouldThrowInvalidOperationException()
    {
        var geofence = DefineGeofence();
        geofence.Activate(TestActor, TestCorrelationId, TestCausationId);

        var act = () => geofence.Activate(TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RecordViolation_WhenActive_ShouldIncrementCount_AndRaiseEvent()
    {
        var geofence = DefineGeofence();
        geofence.Activate(TestActor, TestCorrelationId, TestCausationId);
        var assetId = Guid.NewGuid();

        geofence.RecordViolation(assetId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        geofence.ViolationCount.Should().Be(1);
        geofence.DomainEvents.Should().Contain(e => e.GetType().Name == "GeofenceViolationDetectedEvent");
    }

    [Fact]
    public void RecordViolation_WhenDraft_ShouldThrowInvalidOperationException()
    {
        var geofence = DefineGeofence();

        var act = () => geofence.RecordViolation(Guid.NewGuid(), FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ScheduleAudit_WhenActive_ShouldTransitionToUnderAudit_AndSetDueDate()
    {
        var geofence = DefineGeofence();
        geofence.Activate(TestActor, TestCorrelationId, TestCausationId);
        var dueAt = UtcTimestamp.From(FixedUtcNow.AddDays(30));

        geofence.ScheduleAudit(dueAt, TestActor, TestCorrelationId, TestCausationId);

        geofence.Status.Should().Be(OperatingGeofenceStatus.UnderAudit);
        geofence.NextAuditDueAtUtc.Should().Be(dueAt);
    }

    [Fact]
    public void RecordAudit_WhenUnderAudit_ShouldReturnToActive_AndClearDueDate_AndRaiseEvent()
    {
        var geofence = DefineGeofence();
        geofence.Activate(TestActor, TestCorrelationId, TestCausationId);
        geofence.ScheduleAudit(UtcTimestamp.From(FixedUtcNow.AddDays(30)), TestActor, TestCorrelationId, TestCausationId);

        geofence.RecordAudit(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        geofence.Status.Should().Be(OperatingGeofenceStatus.Active);
        geofence.NextAuditDueAtUtc.Should().BeNull();
        geofence.LastAuditedAtUtc.Should().Be(FixedTimestamp);
        geofence.DomainEvents.Should().Contain(e => e.GetType().Name == "OperatingGeofenceAuditedEvent");
    }

    [Fact]
    public void Retire_WhenActive_ShouldTransitionToRetired()
    {
        var geofence = DefineGeofence();
        geofence.Activate(TestActor, TestCorrelationId, TestCausationId);

        geofence.Retire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        geofence.Status.Should().Be(OperatingGeofenceStatus.Retired);
    }

    [Fact]
    public void Retire_WhenAlreadyRetired_ShouldThrowInvalidStatusTransitionException()
    {
        var geofence = DefineGeofence();
        geofence.Retire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => geofence.Retire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<OperatingGeofenceInvalidStatusTransitionException>();
    }
}

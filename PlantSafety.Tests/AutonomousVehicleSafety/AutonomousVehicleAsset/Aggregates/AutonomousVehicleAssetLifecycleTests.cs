// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.AutonomousVehicleSafety.AutonomousVehicleAsset
//  Layer:          Tests (Unit)
//  File:           AutonomousVehicleAssetLifecycleTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Lifecycle/invariant tests for the AutonomousVehicleAsset aggregate root:
//                  Create/RecordModeTransition/Suspend/Reactivate/Retire transitions and their
//                  guard rules. Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleAsset.Enums;
using PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleAsset.Events;
using PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleAsset.Exceptions;
using Agg = PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleAsset.Aggregates.AutonomousVehicleAsset;

namespace PlantSafety.Tests.AutonomousVehicleSafety.AutonomousVehicleAsset.Aggregates;

public sealed class AutonomousVehicleAssetLifecycleTests : UnitTestBase
{
    private static Agg CreateAsset(
        AutonomousVehicleType vehicleType = AutonomousVehicleType.Amr,
        AutonomyLevel autonomyLevel = AutonomyLevel.SemiAutonomous) =>
        Agg.Create(
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            assetTag: "AMR-001",
            vehicleType: vehicleType,
            autonomyLevel: autonomyLevel);

    [Fact]
    public void Create_ShouldInitializeWithRegisteredStatusAndManualMode_AndRaiseRegisteredEvent()
    {
        var asset = CreateAsset();

        asset.Status.Should().Be(AutonomousVehicleAssetStatus.Registered);
        asset.CurrentAutonomyMode.Should().Be(AutonomyMode.Manual);
        asset.ModeTransitionCount.Should().Be(0);
        asset.AssetTag.Should().Be("AMR-001");
        asset.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<AutonomousVehicleAssetRegisteredEvent>();
    }

    [Fact]
    public void RecordModeTransition_FromRegistered_ShouldActivateAsset_AndIncrementCount()
    {
        var asset = CreateAsset();

        asset.RecordModeTransition(AutonomyMode.Autonomous, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        asset.Status.Should().Be(AutonomousVehicleAssetStatus.Active);
        asset.CurrentAutonomyMode.Should().Be(AutonomyMode.Autonomous);
        asset.ModeTransitionCount.Should().Be(1);
    }

    [Fact]
    public void RecordModeTransition_WhenActive_ShouldRemainActive_AndIncrementCountAgain()
    {
        var asset = CreateAsset();
        asset.RecordModeTransition(AutonomyMode.Teleoperated, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        asset.RecordModeTransition(AutonomyMode.Autonomous, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        asset.Status.Should().Be(AutonomousVehicleAssetStatus.Active);
        asset.ModeTransitionCount.Should().Be(2);
    }

    [Fact]
    public void RecordModeTransition_WhenRetired_ShouldThrowInvalidOperationException()
    {
        var asset = CreateAsset();
        asset.Retire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => asset.RecordModeTransition(AutonomyMode.Autonomous, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Suspend_WhenActive_ShouldTransitionToSuspended_AndRecordReason()
    {
        var asset = CreateAsset();
        asset.RecordModeTransition(AutonomyMode.Autonomous, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        asset.Suspend("Repeated geofence violations", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        asset.Status.Should().Be(AutonomousVehicleAssetStatus.Suspended);
        asset.SuspensionReason.Should().Be("Repeated geofence violations");
        asset.SuspendedAtUtc.Should().Be(FixedTimestamp);
    }

    [Fact]
    public void Suspend_WhenStillRegistered_ShouldThrowInvalidOperationException()
    {
        var asset = CreateAsset();

        var act = () => asset.Suspend("reason", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Reactivate_WhenSuspended_ShouldTransitionToActive_AndClearReason()
    {
        var asset = CreateAsset();
        asset.RecordModeTransition(AutonomyMode.Autonomous, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        asset.Suspend("reason", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        asset.Reactivate(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        asset.Status.Should().Be(AutonomousVehicleAssetStatus.Active);
        asset.SuspensionReason.Should().BeNull();
    }

    [Fact]
    public void Reactivate_WhenNotSuspended_ShouldThrowInvalidOperationException()
    {
        var asset = CreateAsset();

        var act = () => asset.Reactivate(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Retire_FromRegistered_ShouldTransitionToRetired_AndRaiseEvent()
    {
        var asset = CreateAsset();

        asset.Retire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        asset.Status.Should().Be(AutonomousVehicleAssetStatus.Retired);
        asset.DomainEvents.Should().Contain(e => e.GetType().Name == "AutonomousVehicleAssetRetiredEvent");
    }

    [Fact]
    public void Retire_WhenAlreadyRetired_ShouldThrowInvalidStatusTransitionException()
    {
        var asset = CreateAsset();
        asset.Retire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => asset.Retire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<AutonomousVehicleAssetInvalidStatusTransitionException>();
    }

    [Fact]
    public void ClearDomainEvents_ShouldEmptyEventCollection()
    {
        var asset = CreateAsset();
        asset.DomainEvents.Should().NotBeEmpty();

        asset.ClearDomainEvents();

        asset.DomainEvents.Should().BeEmpty();
    }
}

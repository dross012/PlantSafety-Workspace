// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.DigitalTwin.EvacuationModeling.Domain
//  Layer:          Tests (Unit)
//  File:           EgressSimulationRunAggregateTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Lifecycle/invariant unit tests for the EgressSimulationRun aggregate root
//                  (DigitalTwin.EvacuationModeling.EgressSimulations). Isolated, deterministic,
//                  <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.Aggregates;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.Enums;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.Events;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.ValueObjects.Ids;
using PlantSafety.Tests.Common.Extensions;

namespace PlantSafety.Tests.DigitalTwin.EvacuationModeling.Domain;

/// <summary>Unit tests for <see cref="EgressSimulationRun"/> in <c>DigitalTwin.EvacuationModeling.EgressSimulations</c>.</summary>
public sealed class EgressSimulationRunAggregateTests : UnitTestBase
{
    private static EgressSimulationRun CreateRun(EgressSimulationTrigger trigger = EgressSimulationTrigger.Manual) =>
        EgressSimulationRun.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value,
            siteId: null,
            zoneId: EvacuationZoneId.New(),
            twinModelVersion: "v2026.1.0",
            triggerReason: trigger);

    [Fact]
    public void Create_ShouldReturnAggregateWithRunningStatus()
    {
        var run = CreateRun();
        run.Status.Should().Be(EgressSimulationStatus.Running);
    }

    [Fact]
    public void Create_ShouldRaiseStartedEvent()
    {
        var run = CreateRun(EgressSimulationTrigger.QuarterlyFullSite);
        var evt = run.DomainEvents.ShouldRaiseSingleEvent<EgressSimulationStartedEvent>();
        evt.TriggerReason.Should().Be(EgressSimulationTrigger.QuarterlyFullSite);
    }

    [Fact]
    public void Create_WithBlankTwinModelVersion_ShouldThrow()
    {
        var act = () => EgressSimulationRun.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, twinModelVersion: " ");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Complete_OnRunningSimulation_ShouldTransitionToCompletedAndRecordResults()
    {
        var run = CreateRun();

        run.Complete(305, predictedPeakOccupancy: 42, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        run.Status.Should().Be(EgressSimulationStatus.Completed);
        run.SimulatedEvacuationTimeSeconds.Should().Be(305);
        run.PredictedPeakOccupancy.Should().Be(42);
        run.CompletedAtUtc.Should().Be(FixedTimestamp);
    }

    [Fact]
    public void Complete_ShouldRaiseCompletedEvent()
    {
        var run = CreateRun();
        run.ClearDomainEvents();

        run.Complete(200, null, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var evt = run.DomainEvents.ShouldRaiseSingleEvent<EgressSimulationCompletedEvent>();
        evt.SimulatedEvacuationTimeSeconds.Should().Be(200);
    }

    [Fact]
    public void Complete_WithNegativeEvacuationTime_ShouldThrow()
    {
        var run = CreateRun();

        var act = () => run.Complete(-1, null, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Complete_AlreadyCompleted_ShouldThrow()
    {
        var run = CreateRun();
        run.Complete(100, null, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => run.Complete(120, null, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Fail_OnRunningSimulation_ShouldTransitionToFailed()
    {
        var run = CreateRun();

        run.Fail(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        run.Status.Should().Be(EgressSimulationStatus.Failed);
        run.CompletedAtUtc.Should().Be(FixedTimestamp);
        run.SimulatedEvacuationTimeSeconds.Should().BeNull();
    }

    [Fact]
    public void Fail_ShouldRaiseFailedEvent()
    {
        var run = CreateRun();
        run.ClearDomainEvents();

        run.Fail(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        run.DomainEvents.ShouldRaiseSingleEvent<EgressSimulationFailedEvent>();
    }

    [Fact]
    public void Fail_AlreadyCompleted_ShouldThrow()
    {
        var run = CreateRun();
        run.Complete(100, null, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => run.Fail(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void LinkToDrill_ShouldSetLinkedDrillIdAndRaiseEvent()
    {
        var run = CreateRun();
        run.Complete(100, null, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        run.ClearDomainEvents();
        var drillId = Guid.NewGuid();

        run.LinkToDrill(drillId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        run.LinkedDrillId.Should().Be(drillId);
        var evt = run.DomainEvents.ShouldRaiseSingleEvent<EgressSimulationLinkedToDrillEvent>();
        evt.DrillId.Should().Be(drillId);
    }

    [Fact]
    public void LinkToDrill_WithEmptyDrillId_ShouldThrow()
    {
        var run = CreateRun();

        var act = () => run.LinkToDrill(Guid.Empty, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ClearDomainEvents_ShouldEmptyList()
    {
        var run = CreateRun();
        run.ClearDomainEvents();
        run.DomainEvents.Should().BeEmpty();
    }
}

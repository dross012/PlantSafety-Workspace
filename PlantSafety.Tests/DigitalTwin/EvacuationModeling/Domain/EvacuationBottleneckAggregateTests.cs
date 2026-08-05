// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.DigitalTwin.EvacuationModeling.Domain
//  Layer:          Tests (Unit)
//  File:           EvacuationBottleneckAggregateTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Lifecycle/invariant unit tests for the EvacuationBottleneck aggregate root
//                  (DigitalTwin.EvacuationModeling.EvacuationBottlenecks). Isolated, deterministic,
//                  <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.Aggregates;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.Enums;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.Events;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.ValueObjects.Ids;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.ValueObjects.Ids;
using PlantSafety.Tests.Common.Extensions;

namespace PlantSafety.Tests.DigitalTwin.EvacuationModeling.Domain;

/// <summary>Unit tests for <see cref="EvacuationBottleneck"/> in <c>DigitalTwin.EvacuationModeling.EvacuationBottlenecks</c>.</summary>
public sealed class EvacuationBottleneckAggregateTests : UnitTestBase
{
    private static EvacuationBottleneck CreateBottleneck(int delaySeconds = 90) =>
        EvacuationBottleneck.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value,
            siteId: null,
            routeId: EvacuationRouteId.New(),
            simulationRunId: EgressSimulationRunId.New(),
            bottleneckLocation: "Stairwell B, Level 2 landing",
            delaySeconds: delaySeconds);

    [Fact]
    public void Create_ShouldReturnAggregateWithDetectedStatus()
    {
        var bottleneck = CreateBottleneck();
        bottleneck.Status.Should().Be(EvacuationBottleneckStatus.Detected);
        bottleneck.RecurrenceCount.Should().Be(0);
    }

    [Fact]
    public void Create_ShouldRaiseDetectedEvent()
    {
        var bottleneck = CreateBottleneck(delaySeconds: 120);
        var evt = bottleneck.DomainEvents.ShouldRaiseSingleEvent<EvacuationBottleneckDetectedEvent>();
        evt.DelaySeconds.Should().Be(120);
        evt.EvacuationBottleneckId.Should().Be(bottleneck.EvacuationBottleneckId);
    }

    [Fact]
    public void RecordRecurrence_ShouldIncrementRecurrenceCountAndTransitionToRecurred()
    {
        var bottleneck = CreateBottleneck();
        var newRunId = EgressSimulationRunId.New();

        bottleneck.RecordRecurrence(newRunId, 150, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        bottleneck.Status.Should().Be(EvacuationBottleneckStatus.Recurred);
        bottleneck.RecurrenceCount.Should().Be(1);
        bottleneck.DelaySeconds.Should().Be(150);
        bottleneck.SimulationRunId.Should().Be(newRunId);
    }

    [Fact]
    public void RecordRecurrence_ShouldRaiseRecurredEventWithAccumulatedCount()
    {
        var bottleneck = CreateBottleneck();
        bottleneck.RecordRecurrence(EgressSimulationRunId.New(), 100, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        bottleneck.ClearDomainEvents();

        bottleneck.RecordRecurrence(EgressSimulationRunId.New(), 200, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var evt = bottleneck.DomainEvents.ShouldRaiseSingleEvent<EvacuationBottleneckRecurredEvent>();
        evt.RecurrenceCount.Should().Be(2);
    }

    [Fact]
    public void MarkMitigated_ShouldTransitionToMitigatedAndRaiseEvent()
    {
        var bottleneck = CreateBottleneck();
        bottleneck.ClearDomainEvents();

        bottleneck.MarkMitigated(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        bottleneck.Status.Should().Be(EvacuationBottleneckStatus.Mitigated);
        bottleneck.DomainEvents.ShouldRaiseSingleEvent<EvacuationBottleneckMitigatedEvent>();
    }

    [Fact]
    public void MarkMitigated_AfterRecurrence_ShouldStillTransitionToMitigated()
    {
        var bottleneck = CreateBottleneck();
        bottleneck.RecordRecurrence(EgressSimulationRunId.New(), 300, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        bottleneck.MarkMitigated(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        bottleneck.Status.Should().Be(EvacuationBottleneckStatus.Mitigated);
        bottleneck.RecurrenceCount.Should().Be(1, "mitigation must not reset the historical recurrence count");
    }

    [Fact]
    public void ClearDomainEvents_ShouldEmptyList()
    {
        var bottleneck = CreateBottleneck();
        bottleneck.ClearDomainEvents();
        bottleneck.DomainEvents.Should().BeEmpty();
    }
}

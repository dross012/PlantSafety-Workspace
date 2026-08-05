// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.DigitalTwin.EvacuationModeling.TestMatrix
//  Layer:          Tests (Unit) -- Acceptance (UAT)
//  File:           AcceptanceTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Acceptance (UAT) test type: asserts business-facing acceptance criteria for the
//                  module in Given/When/Then form, one AC per aggregate's core business promise.
//                  Isolated, deterministic, no I/O.
// ============================================================================
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.Aggregates;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.Enums;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.ValueObjects.Ids;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.Aggregates;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.Enums;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.ValueObjects.Ids;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.Aggregates;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.Services;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.Aggregates;

namespace PlantSafety.Tests.DigitalTwin.EvacuationModeling.TestMatrix;

public sealed class AcceptanceTests : UnitTestBase
{
    [Fact]
    public void AC01_GivenARouteDefinedAgainstAnUnvalidatedModelVersion_ThenTheRouteIsNeverCreated()
    {
        // GIVEN a caller attempts to define a route without a validated Digital Twin model version
        // WHEN Create is invoked with a blank TwinModelVersion
        var act = () => EvacuationRoute.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, twinModelVersion: "");

        // THEN the route must never be created -- geometry must come only from a validated model
        act.Should().Throw<ArgumentException>(
            "route geometry must never be sourced from an unvalidated Digital Twin model version");
    }

    [Fact]
    public void AC02_GivenAnObstructedRoute_WhenTheObstructionIsCleared_ThenTheRouteBecomesUsableAgain()
    {
        // GIVEN an evacuation route with an open obstruction
        var route = EvacuationRoute.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, zoneId: EvacuationZoneId.New(), twinModelVersion: "v2026.1.0");
        var obstructionId = RouteObstructionId.New();
        route.RecordObstruction(obstructionId, "Debris", ObstructionSeverity.Severe, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        // WHEN the obstruction is cleared
        route.ClearObstruction(obstructionId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        // THEN the route becomes usable again -- this is the module's core safety promise
        route.Status.Should().Be(EvacuationRouteStatus.Active);
    }

    [Fact]
    public void AC03_GivenAZoneApproachingCapacity_WhenOccupancyExceedsThreshold_ThenAnAlertConditionIsRaised()
    {
        // GIVEN a zone with a configured occupancy threshold
        var zone = EvacuationZone.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, zoneName: "Zone A", twinModelVersion: "v2026.1.0", zoneCapacity: 200, occupancyThreshold: 150);

        // WHEN a recorded headcount exceeds that threshold
        zone.RecordOccupancySnapshot(OccupancyReadingId.New(), 190, OccupancyReadingSource.BmsFeed,
            FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        // THEN the zone status reflects the breach so AlertRoutingService can escalate it
        zone.Status.Should().Be(EvacuationZoneStatus.OccupancyThresholdBreached);
    }

    [Fact]
    public void AC04_GivenACompletedSimulationThatMateriallyExceedsARoutesEstimatedEgressTime_ThenABottleneckIsAutomaticallyDetected()
    {
        // GIVEN a completed egress-simulation run that took materially longer than the route's estimate
        var run = EgressSimulationRun.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, zoneId: EvacuationZoneId.New(), twinModelVersion: "v2026.1.0");
        run.Complete(500, null, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        // WHEN the Bottleneck Analysis subdomain evaluates the run against a route's 300s estimate
        var detected = new EgressSimulationBottleneckAnalysisService().TryDetectBottleneck(run, routeEstimatedEgressTimeSeconds: 300, out var delaySeconds);

        // THEN the platform automatically flags a bottleneck rather than requiring a human to notice
        detected.Should().BeTrue("a 200s overrun materially exceeds the estimate and must be surfaced automatically");
        delaySeconds.Should().Be(200);
    }

    [Fact]
    public void AC05_GivenARecurringBottleneck_ThenTheRecurrenceCountAccumulatesForTheKpi()
    {
        // GIVEN a previously detected bottleneck
        var bottleneck = EvacuationBottleneck.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value,
            routeId: PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.ValueObjects.Ids.EvacuationRouteId.New(),
            simulationRunId: PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.ValueObjects.Ids.EgressSimulationRunId.New(),
            bottleneckLocation: "Stairwell B", delaySeconds: 90);

        // WHEN it is detected again on a later simulation run
        bottleneck.RecordRecurrence(
            PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.ValueObjects.Ids.EgressSimulationRunId.New(),
            150, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        // THEN the recurrence count feeds the bottleneck-recurrence-rate KPI
        bottleneck.RecurrenceCount.Should().Be(1);
        bottleneck.Status.Should().Be(PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.Enums.EvacuationBottleneckStatus.Recurred);
    }
}

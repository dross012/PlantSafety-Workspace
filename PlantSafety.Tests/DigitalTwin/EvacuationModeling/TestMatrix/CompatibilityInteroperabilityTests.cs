// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.DigitalTwin.EvacuationModeling.TestMatrix
//  Layer:          Tests (Unit) -- Compatibility / Interoperability
//  File:           CompatibilityInteroperabilityTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Compatibility and Interoperability test types. Compatibility: the module's
//                  enum string values (the command wire/persistence contract for Severity/Source/
//                  TriggerReason) round-trip stably through the handlers' Enum.TryParse calls, and
//                  an unrecognized/legacy string value degrades to the documented safe default
//                  rather than throwing. Interoperability: EvacuationBottleneck correctly
//                  cross-references EvacuationRoute and EgressSimulationRun identities from
//                  sibling aggregates by Guid, and status/severity enum ToString() values match the
//                  read-model DTO contract exactly. Isolated, deterministic, no I/O.
// ============================================================================
using PlantSafety.Application.DigitalTwin.EvacuationModeling.Commands;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.Services;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.Aggregates;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.Enums;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.Enums;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.ValueObjects.Ids;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.Enums;
using PlantSafety.Tests.DigitalTwin.EvacuationModeling.TestMatrix.Fakes;

namespace PlantSafety.Tests.DigitalTwin.EvacuationModeling.TestMatrix;

public sealed class CompatibilityInteroperabilityTests : UnitTestBase
{
    [Theory]
    [InlineData("Minor")]
    [InlineData("Moderate")]
    [InlineData("Severe")]
    public async Task RecordRouteObstruction_AllPersistedSeverityStrings_RoundTripThroughEnumTryParse(string severityCode)
    {
        var repo = new InMemoryEvacuationRouteRepository();
        var defineHandler = new DefineEvacuationRouteCommandHandler(repo, NullLogger<DefineEvacuationRouteCommandHandler>());
        var defined = await defineHandler.Handle(new DefineEvacuationRouteCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            "Route", Guid.NewGuid(), Guid.NewGuid(), "geo://1", "v2026.1.0", 60, TestUserId.Value), CancellationToken.None);

        var handler = new RecordRouteObstructionCommandHandler(repo, NullLogger<RecordRouteObstructionCommandHandler>());
        await handler.Handle(new RecordRouteObstructionCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            defined.Value.EvacuationRouteId, "Obstruction", severityCode, TestUserId.Value), CancellationToken.None);

        var route = await repo.GetByIdAsync(EvacuationRouteId.FromGuid(defined.Value.EvacuationRouteId), CancellationToken.None);
        route!.ObstructionHistory.Single().Severity.Should().Be(Enum.Parse<ObstructionSeverity>(severityCode));
    }

    [Theory]
    [InlineData("BmsFeed")]
    [InlineData("Sensor")]
    [InlineData("Manual")]
    [InlineData("SimulationDerived")]
    public async Task RecordOccupancySnapshot_AllPersistedSourceStrings_RoundTripThroughEnumTryParse(string sourceCode)
    {
        var repo = new InMemoryEvacuationZoneRepository();
        var defineHandler = new DefineEvacuationZoneCommandHandler(repo, NullLogger<DefineEvacuationZoneCommandHandler>());
        var defined = await defineHandler.Handle(new DefineEvacuationZoneCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            "Zone", "v2026.1.0", 200, 150, TestUserId.Value), CancellationToken.None);

        var handler = new RecordOccupancySnapshotCommandHandler(repo, NullLogger<RecordOccupancySnapshotCommandHandler>());
        await handler.Handle(new RecordOccupancySnapshotCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            defined.Value.EvacuationZoneId, 10, sourceCode, TestUserId.Value), CancellationToken.None);

        var zone = await repo.GetByIdAsync(
            PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.ValueObjects.Ids.EvacuationZoneId.FromGuid(defined.Value.EvacuationZoneId),
            CancellationToken.None);
        zone!.OccupancyReadings.Single().Source.Should().Be(Enum.Parse<OccupancyReadingSource>(sourceCode));
    }

    [Fact]
    public async Task RecordRouteObstruction_WithLegacyRetiredSeverityString_DoesNotThrow_DegradesToModerateDefault()
    {
        // Compatibility guard: a legacy/unrecognized severity string from an older caller must not
        // crash the handler -- RecordRouteObstructionCommandHandler's documented fallback is
        // ObstructionSeverity.Moderate, never a thrown exception.
        var repo = new InMemoryEvacuationRouteRepository();
        var defineHandler = new DefineEvacuationRouteCommandHandler(repo, NullLogger<DefineEvacuationRouteCommandHandler>());
        var defined = await defineHandler.Handle(new DefineEvacuationRouteCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            "Route", Guid.NewGuid(), Guid.NewGuid(), "geo://1", "v2026.1.0", 60, TestUserId.Value), CancellationToken.None);

        var handler = new RecordRouteObstructionCommandHandler(repo, NullLogger<RecordRouteObstructionCommandHandler>());
        var result = await handler.Handle(new RecordRouteObstructionCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            defined.Value.EvacuationRouteId, "Obstruction", "LegacyCritical", TestUserId.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var route = await repo.GetByIdAsync(EvacuationRouteId.FromGuid(defined.Value.EvacuationRouteId), CancellationToken.None);
        route!.ObstructionHistory.Single().Severity.Should().Be(ObstructionSeverity.Moderate);
    }

    [Fact]
    public async Task EvacuationBottleneck_InteroperatesWithRouteAndSimulationRun_ViaGuidCorrelation()
    {
        // Interoperability: EvacuationBottleneck does not hold typed references beyond its own
        // module's strong IDs, but RouteId and SimulationRunId must correctly carry the sibling
        // aggregates' real identity values end-to-end so downstream dashboards can join across
        // EvacuationRoutes, EgressSimulations, and EvacuationBottlenecks.
        var routeRepo = new InMemoryEvacuationRouteRepository();
        var defineRouteHandler = new DefineEvacuationRouteCommandHandler(routeRepo, NullLogger<DefineEvacuationRouteCommandHandler>());
        var route = await defineRouteHandler.Handle(new DefineEvacuationRouteCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            "Route", Guid.NewGuid(), Guid.NewGuid(), "geo://1", "v2026.1.0", 100, TestUserId.Value), CancellationToken.None);

        var simulationRepo = new InMemoryEgressSimulationRunRepository();
        var bottleneckRepo = new InMemoryEvacuationBottleneckRepository();
        var runHandler = new RunEgressSimulationCommandHandler(
            simulationRepo, routeRepo, bottleneckRepo, new EgressSimulationBottleneckAnalysisService(),
            NullLogger<RunEgressSimulationCommandHandler>());

        var run = await runHandler.Handle(new RunEgressSimulationCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            Guid.NewGuid(), route.Value.EvacuationRouteId, "v2026.1.0", "Manual", 500, null, TestUserId.Value), CancellationToken.None);

        var bottleneck = (await bottleneckRepo.GetOpenForRouteAsync(EvacuationRouteId.FromGuid(route.Value.EvacuationRouteId), CancellationToken.None))!;

        bottleneck.RouteId.Value.Should().Be(route.Value.EvacuationRouteId);
        bottleneck.SimulationRunId.Value.Should().Be(run.Value.EgressSimulationRunId);
    }

    [Fact]
    public void EvacuationBottleneckStatus_ToString_MatchesReadModelDtoStringContract()
    {
        // The Infrastructure read repository persists Status.ToString() verbatim into
        // EvacuationBottleneckItemDto.BottleneckStatus -- these three literal strings are the wire
        // contract external API consumers depend on and must never silently change spelling.
        EvacuationBottleneckStatus.Detected.ToString().Should().Be("Detected");
        EvacuationBottleneckStatus.Recurred.ToString().Should().Be("Recurred");
        EvacuationBottleneckStatus.Mitigated.ToString().Should().Be("Mitigated");
    }
}

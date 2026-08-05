// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.DigitalTwin.EvacuationModeling.TestMatrix
//  Layer:          Tests (Unit) -- Resilience / Chaos
//  File:           ResilienceChaosTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Resilience and Chaos test types: injects simulated repository/infrastructure
//                  faults (transient exceptions) into each command handler and verifies the
//                  handler degrades gracefully to a Result.Failure with a sanitized error message
//                  rather than propagating an unhandled exception or leaking infrastructure
//                  details. Isolated, deterministic, no I/O.
// ============================================================================
using PlantSafety.Application.DigitalTwin.EvacuationModeling.Commands;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.Services;
using PlantSafety.Tests.DigitalTwin.EvacuationModeling.TestMatrix.Fakes;

namespace PlantSafety.Tests.DigitalTwin.EvacuationModeling.TestMatrix;

public sealed class ResilienceChaosTests : UnitTestBase
{
    [Fact]
    public async Task DefineEvacuationRoute_UnderSimulatedTransientDatabaseFault_DegradesGracefully()
    {
        var repo = new InMemoryEvacuationRouteRepository { FaultOnAdd = new TimeoutException("Simulated transient SQL timeout.") };
        var handler = new DefineEvacuationRouteCommandHandler(repo, NullLogger<DefineEvacuationRouteCommandHandler>());

        var result = await handler.Handle(new DefineEvacuationRouteCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            "Route", Guid.NewGuid(), Guid.NewGuid(), "geo://1", "v2026.1.0", 60, TestUserId.Value), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().NotContain("Simulated transient SQL timeout",
            "infrastructure fault details must never leak into the sanitized ApplicationError message");
    }

    [Fact]
    public async Task RecordRouteObstruction_UnderSimulatedTransientUpdateFault_DegradesGracefully()
    {
        var repo = new InMemoryEvacuationRouteRepository();
        var defineHandler = new DefineEvacuationRouteCommandHandler(repo, NullLogger<DefineEvacuationRouteCommandHandler>());
        var defined = await defineHandler.Handle(new DefineEvacuationRouteCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            "Route", Guid.NewGuid(), Guid.NewGuid(), "geo://1", "v2026.1.0", 60, TestUserId.Value), CancellationToken.None);

        repo.FaultOnUpdate = new InvalidOperationException("Simulated connection pool exhaustion.");
        var handler = new RecordRouteObstructionCommandHandler(repo, NullLogger<RecordRouteObstructionCommandHandler>());

        var result = await handler.Handle(new RecordRouteObstructionCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            defined.Value.EvacuationRouteId, "Blocked", "Minor", TestUserId.Value), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("DigitalTwin.EvacuationModeling.EvacuationRoute.RecordObstructionFailed");
    }

    [Fact]
    public async Task DefineEvacuationZone_UnderSimulatedTransientFault_DegradesGracefully()
    {
        var repo = new InMemoryEvacuationZoneRepository { FaultOnAdd = new InvalidOperationException("Simulated outbox write failure.") };
        var handler = new DefineEvacuationZoneCommandHandler(repo, NullLogger<DefineEvacuationZoneCommandHandler>());

        var result = await handler.Handle(new DefineEvacuationZoneCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            "Zone", "v2026.1.0", 100, 80, TestUserId.Value), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("DigitalTwin.EvacuationModeling.EvacuationZone.DefineFailed");
    }

    [Fact]
    public async Task RunEgressSimulation_UnderSimulatedBottleneckRepositoryFault_DegradesGracefully()
    {
        // Chaos: the fault is injected deep in the cross-repository Bottleneck Analysis integration
        // (not the primary simulation write), confirming the handler's single outer try/catch
        // still catches faults raised by any of its three collaborating repositories.
        var routeRepo = new InMemoryEvacuationRouteRepository();
        var defineRouteHandler = new DefineEvacuationRouteCommandHandler(routeRepo, NullLogger<DefineEvacuationRouteCommandHandler>());
        var route = await defineRouteHandler.Handle(new DefineEvacuationRouteCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            "Route", Guid.NewGuid(), Guid.NewGuid(), "geo://1", "v2026.1.0", 100, TestUserId.Value), CancellationToken.None);

        var simulationRepo = new InMemoryEgressSimulationRunRepository();
        var bottleneckRepo = new InMemoryEvacuationBottleneckRepository { FaultOnAdd = new TimeoutException("Simulated bottleneck-table write timeout.") };
        var handler = new RunEgressSimulationCommandHandler(
            simulationRepo, routeRepo, bottleneckRepo, new EgressSimulationBottleneckAnalysisService(),
            NullLogger<RunEgressSimulationCommandHandler>());

        var result = await handler.Handle(new RunEgressSimulationCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            Guid.NewGuid(), route.Value.EvacuationRouteId, "v2026.1.0", "Manual", 500, null, TestUserId.Value), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("DigitalTwin.EvacuationModeling.EgressSimulationRun.RunFailed");
    }

    [Fact]
    public async Task LinkSimulationToDrill_ChaosInjectedUpdateFault_PropagatesAsGenericFailure_NotUnhandled()
    {
        var repo = new InMemoryEgressSimulationRunRepository();
        var startHandler = new RunEgressSimulationCommandHandler(
            repo, new InMemoryEvacuationRouteRepository(), new InMemoryEvacuationBottleneckRepository(),
            new EgressSimulationBottleneckAnalysisService(), NullLogger<RunEgressSimulationCommandHandler>());
        var started = await startHandler.Handle(new RunEgressSimulationCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            Guid.NewGuid(), null, "v2026.1.0", "Manual", 100, null, TestUserId.Value), CancellationToken.None);

        repo.FaultOnUpdate = new InvalidOperationException("Simulated deadlock victim.");
        var linkHandler = new LinkSimulationToDrillCommandHandler(repo, NullLogger<LinkSimulationToDrillCommandHandler>());

        var result = await linkHandler.Handle(new LinkSimulationToDrillCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            started.Value.EgressSimulationRunId, Guid.NewGuid(), TestUserId.Value), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("DigitalTwin.EvacuationModeling.EgressSimulationRun.LinkToDrillFailed");
    }
}

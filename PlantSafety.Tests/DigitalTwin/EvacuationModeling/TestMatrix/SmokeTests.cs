// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.DigitalTwin.EvacuationModeling.TestMatrix
//  Layer:          Tests (Unit) -- Smoke
//  File:           SmokeTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Smoke test type: fast, shallow sanity checks that the module's core building
//                  blocks construct and execute their trivial happy path without throwing -- one
//                  smoke check per validator family and per command handler, scaled to this
//                  4-aggregate/8-command module. Not a substitute for the deeper Functional suite.
//                  Isolated, deterministic, no I/O.
// ============================================================================
using PlantSafety.Application.DigitalTwin.EvacuationModeling.Commands;
using PlantSafety.Application.DigitalTwin.EvacuationModeling.Validation;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.Services;
using PlantSafety.Tests.DigitalTwin.EvacuationModeling.TestMatrix.Fakes;

namespace PlantSafety.Tests.DigitalTwin.EvacuationModeling.TestMatrix;

public sealed class SmokeTests : UnitTestBase
{
    [Fact]
    public void AllEightValidators_CanBeConstructedWithoutThrowing()
    {
        var act = () =>
        {
            _ = new DefineEvacuationRouteCommandValidator();
            _ = new RetireEvacuationRouteCommandValidator();
            _ = new RecordRouteObstructionCommandValidator();
            _ = new ClearRouteObstructionCommandValidator();
            _ = new DefineEvacuationZoneCommandValidator();
            _ = new RecordOccupancySnapshotCommandValidator();
            _ = new RunEgressSimulationCommandValidator();
            _ = new LinkSimulationToDrillCommandValidator();
        };

        act.Should().NotThrow();
    }

    [Fact]
    public async Task DefineEvacuationRouteCommandHandler_HappyPath_DoesNotThrow()
    {
        var handler = new DefineEvacuationRouteCommandHandler(new InMemoryEvacuationRouteRepository(), NullLogger<DefineEvacuationRouteCommandHandler>());

        var act = () => handler.Handle(new DefineEvacuationRouteCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            "Smoke Route", Guid.NewGuid(), Guid.NewGuid(), "geo://smoke", "v2026.1.0", 60, TestUserId.Value), CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DefineEvacuationZoneCommandHandler_HappyPath_DoesNotThrow()
    {
        var handler = new DefineEvacuationZoneCommandHandler(new InMemoryEvacuationZoneRepository(), NullLogger<DefineEvacuationZoneCommandHandler>());

        var act = () => handler.Handle(new DefineEvacuationZoneCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            "Smoke Zone", "v2026.1.0", 100, 80, TestUserId.Value), CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RunEgressSimulationCommandHandler_HappyPath_DoesNotThrow()
    {
        var handler = new RunEgressSimulationCommandHandler(
            new InMemoryEgressSimulationRunRepository(), new InMemoryEvacuationRouteRepository(),
            new InMemoryEvacuationBottleneckRepository(), new EgressSimulationBottleneckAnalysisService(),
            NullLogger<RunEgressSimulationCommandHandler>());

        var act = () => handler.Handle(new RunEgressSimulationCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            Guid.NewGuid(), null, "v2026.1.0", "Manual", 100, null, TestUserId.Value), CancellationToken.None);

        await act.Should().NotThrowAsync();
    }
}

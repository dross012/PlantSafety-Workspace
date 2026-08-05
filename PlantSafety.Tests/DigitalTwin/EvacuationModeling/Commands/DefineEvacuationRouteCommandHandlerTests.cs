// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.DigitalTwin.EvacuationModeling.Commands
//  Layer:          Tests (Unit)
//  File:           DefineEvacuationRouteCommandHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Unit tests for DefineEvacuationRouteCommandHandler. Isolated, deterministic,
//                  <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.DigitalTwin.EvacuationModeling.Commands;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.Aggregates;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.Interfaces;

namespace PlantSafety.Tests.DigitalTwin.EvacuationModeling.Commands;

public sealed class DefineEvacuationRouteCommandHandlerTests : UnitTestBase
{
    private readonly IEvacuationRouteRepository _repo = Substitute.For<IEvacuationRouteRepository>();

    private DefineEvacuationRouteCommandHandler Sut() => new(_repo, NullLogger<DefineEvacuationRouteCommandHandler>());

    private static DefineEvacuationRouteCommand ValidCommand() => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: TestCorrelationId.Value,
        CausationId: TestCausationId.Value,
        TenantId: TestTenantId.Value,
        SiteId: null,
        RouteName: "Corridor A -> Assembly Point 1",
        ZoneId: Guid.NewGuid(),
        AssemblyPointId: Guid.NewGuid(),
        PathGeometryRef: "geo://twin/path/1",
        TwinModelVersion: "v2026.1.0",
        EstimatedEgressTimeSeconds: 120,
        PerformedByUserId: TestUserId.Value);

    [Fact]
    public async Task Handle_WithValidCommand_ReturnsSuccessAndPersistsAggregate()
    {
        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.EvacuationRouteId.Should().NotBe(Guid.Empty);
        result.Value.Status.Should().Be("Active");
        await _repo.Received(1).AddAsync(Arg.Is<EvacuationRoute>(a => a.RouteName == "Corridor A -> Assembly Point 1"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsFailureWithoutPropagatingException()
    {
        _repo.AddAsync(Arg.Any<EvacuationRoute>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException("transient DB failure"));

        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("DigitalTwin.EvacuationModeling.EvacuationRoute.DefineFailed");
    }

    [Fact]
    public async Task Handle_ShouldPropagateTenantIdOntoAggregate()
    {
        EvacuationRoute? captured = null;
        _repo.AddAsync(Arg.Do<EvacuationRoute>(a => captured = a), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        await Sut().Handle(ValidCommand(), CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.TenantId.Should().Be(TestTenantId.Value);
    }
}

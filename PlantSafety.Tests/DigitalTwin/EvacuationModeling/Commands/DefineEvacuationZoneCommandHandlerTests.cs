// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.DigitalTwin.EvacuationModeling.Commands
//  Layer:          Tests (Unit)
//  File:           DefineEvacuationZoneCommandHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Unit tests for DefineEvacuationZoneCommandHandler. Isolated, deterministic,
//                  <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.DigitalTwin.EvacuationModeling.Commands;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.Aggregates;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.Interfaces;

namespace PlantSafety.Tests.DigitalTwin.EvacuationModeling.Commands;

public sealed class DefineEvacuationZoneCommandHandlerTests : UnitTestBase
{
    private readonly IEvacuationZoneRepository _repo = Substitute.For<IEvacuationZoneRepository>();

    private DefineEvacuationZoneCommandHandler Sut() => new(_repo, NullLogger<DefineEvacuationZoneCommandHandler>());

    private static DefineEvacuationZoneCommand ValidCommand() => new(
        Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
        "Production Floor A", "v2026.1.0", 200, 150, TestUserId.Value);

    [Fact]
    public async Task Handle_WithValidCommand_ReturnsSuccessAndPersistsAggregate()
    {
        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Active");
        await _repo.Received(1).AddAsync(Arg.Is<EvacuationZone>(z => z.ZoneName == "Production Floor A"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsFailure()
    {
        _repo.AddAsync(Arg.Any<EvacuationZone>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException("transient DB failure"));

        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("DigitalTwin.EvacuationModeling.EvacuationZone.DefineFailed");
    }
}

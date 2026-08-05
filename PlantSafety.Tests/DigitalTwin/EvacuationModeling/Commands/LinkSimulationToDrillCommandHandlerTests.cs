// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.DigitalTwin.EvacuationModeling.Commands
//  Layer:          Tests (Unit)
//  File:           LinkSimulationToDrillCommandHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Unit tests for LinkSimulationToDrillCommandHandler. Isolated, deterministic,
//                  <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.DigitalTwin.EvacuationModeling.Commands;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.Aggregates;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.Interfaces;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.ValueObjects.Ids;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.ValueObjects.Ids;

namespace PlantSafety.Tests.DigitalTwin.EvacuationModeling.Commands;

public sealed class LinkSimulationToDrillCommandHandlerTests : UnitTestBase
{
    private readonly IEgressSimulationRunRepository _repo = Substitute.For<IEgressSimulationRunRepository>();

    private LinkSimulationToDrillCommandHandler Sut() => new(_repo, NullLogger<LinkSimulationToDrillCommandHandler>());

    private static EgressSimulationRun CreatedRun() => EgressSimulationRun.Create(
        TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
        tenantId: TestTenantId.Value, zoneId: EvacuationZoneId.New(), twinModelVersion: "v2026.1.0");

    [Fact]
    public async Task Handle_WithExistingRun_ReturnsSuccessAndLinksDrill()
    {
        var run = CreatedRun();
        _repo.GetByIdAsync(run.EgressSimulationRunId, Arg.Any<CancellationToken>()).Returns(run);
        var drillId = Guid.NewGuid();

        var command = new LinkSimulationToDrillCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            run.EgressSimulationRunId.Value, drillId, TestUserId.Value);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        run.LinkedDrillId.Should().Be(drillId);
        await _repo.Received(1).UpdateAsync(run, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRunNotFound_ReturnsFailureWithNotFoundCode()
    {
        _repo.GetByIdAsync(Arg.Any<EgressSimulationRunId>(), Arg.Any<CancellationToken>()).Returns((EgressSimulationRun?)null);

        var command = new LinkSimulationToDrillCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            Guid.NewGuid(), Guid.NewGuid(), TestUserId.Value);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("DigitalTwin.EvacuationModeling.EgressSimulationRun.NotFound");
    }

    [Fact]
    public async Task Handle_WithEmptyDrillId_ReturnsFailureWithoutPropagatingDomainException()
    {
        var run = CreatedRun();
        _repo.GetByIdAsync(run.EgressSimulationRunId, Arg.Any<CancellationToken>()).Returns(run);

        var command = new LinkSimulationToDrillCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            run.EgressSimulationRunId.Value, Guid.Empty, TestUserId.Value);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("DigitalTwin.EvacuationModeling.EgressSimulationRun.LinkToDrillFailed");
    }
}

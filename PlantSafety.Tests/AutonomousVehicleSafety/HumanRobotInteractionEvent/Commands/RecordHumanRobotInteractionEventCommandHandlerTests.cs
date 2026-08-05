// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.AutonomousVehicleSafety.HumanRobotInteractionEvent.Commands
//  Layer:          Tests (Unit)
//  File:           RecordHumanRobotInteractionEventCommandHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Unit tests for RecordHumanRobotInteractionEventCommandHandler.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.AutonomousVehicleSafety.Commands;
using PlantSafety.Domain.AutonomousVehicleSafety.HumanRobotInteractionEvent.Interfaces;
using Agg = PlantSafety.Domain.AutonomousVehicleSafety.HumanRobotInteractionEvent.Aggregates.HumanRobotInteractionEvent;

namespace PlantSafety.Tests.AutonomousVehicleSafety.HumanRobotInteractionEvent.Commands;

public sealed class RecordHumanRobotInteractionEventCommandHandlerTests : UnitTestBase
{
    private readonly IHumanRobotInteractionEventRepository _repo = Substitute.For<IHumanRobotInteractionEventRepository>();

    private RecordHumanRobotInteractionEventCommandHandler Sut() => new(_repo, NullLogger<RecordHumanRobotInteractionEventCommandHandler>());

    private static RecordHumanRobotInteractionEventCommand ValidCommand() => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: TestCorrelationId.Value,
        CausationId: TestCausationId.Value,
        TenantId: TestTenantId.Value,
        SiteId: null,
        AutonomousVehicleAssetId: Guid.NewGuid(),
        InteractionType: "NearMiss",
        SeverityLevel: "High",
        DistanceMeters: 0.4m,
        LocationDescription: "Loading dock 3",
        PerformedByUserId: TestUserId.Value);

    [Fact]
    public async Task Handle_WithValidCommand_ShouldPersistEvent_AndReturnSuccess()
    {
        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Recorded");
        await _repo.Received(1).AddAsync(Arg.Is<Agg>(a => a.LocationDescription == "Loading dock 3"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithUnrecognizedInteractionType_ShouldDefaultToProximityAlert()
    {
        var command = ValidCommand() with { InteractionType = "NotARealType" };

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).AddAsync(Arg.Is<Agg>(a => a.InteractionType.ToString() == "ProximityAlert"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldReturnFailureWithRecordFailedErrorCode()
    {
        _repo.AddAsync(Arg.Any<Agg>(), Arg.Any<CancellationToken>()).Returns<Task>(_ => throw new InvalidOperationException("db unavailable"));

        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("AutonomousVehicleSafety.HumanRobotInteractionEvent.RecordFailed");
    }
}

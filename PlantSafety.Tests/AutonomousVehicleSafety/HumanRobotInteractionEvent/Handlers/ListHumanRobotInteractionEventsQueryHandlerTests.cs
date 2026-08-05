// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.AutonomousVehicleSafety.HumanRobotInteractionEvent.Handlers
//  Layer:          Tests (Unit)
//  File:           ListHumanRobotInteractionEventsQueryHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Unit tests for ListHumanRobotInteractionEventsQueryHandler.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.AutonomousVehicleSafety.HumanRobotInteractionEvent.Handlers;
using PlantSafety.Application.AutonomousVehicleSafety.HumanRobotInteractionEvent.Models;
using PlantSafety.Application.AutonomousVehicleSafety.HumanRobotInteractionEvent.Queries;
using PlantSafety.Application.AutonomousVehicleSafety.HumanRobotInteractionEvent.Repositories;

namespace PlantSafety.Tests.AutonomousVehicleSafety.HumanRobotInteractionEvent.Handlers;

public sealed class ListHumanRobotInteractionEventsQueryHandlerTests : UnitTestBase
{
    private readonly IHumanRobotInteractionEventRepository _repo = Substitute.For<IHumanRobotInteractionEventRepository>();

    private ListHumanRobotInteractionEventsQueryHandler Sut() => new(_repo, NullLogger<ListHumanRobotInteractionEventsQueryHandler>());

    private static HumanRobotInteractionEventItemDto ItemFor(Guid id) => new()
    {
        HumanRobotInteractionEventId = id,
        TenantId = TestTenantId.Value,
        AutonomousVehicleAssetId = Guid.NewGuid(),
        InteractionType = "NearMiss",
        SeverityLevel = "High",
        Status = "Recorded",
        DetectedAtUtc = FixedUtcNow,
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
    };

    [Fact]
    public async Task Handle_WithMatchingEvents_ShouldReturnItems()
    {
        var from = FixedUtcNow.AddDays(-7);
        var to = FixedUtcNow;
        var items = new List<HumanRobotInteractionEventItemDto> { ItemFor(Guid.NewGuid()) };
        _repo.GetHumanRobotInteractionEventItemsAsync(TestTenantId.Value, null, from, to, Arg.Any<CancellationToken>()).Returns(items);

        var result = await Sut().Handle(new ListHumanRobotInteractionEventsQuery(TestTenantId.Value, null, from, to), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_WhenNoEvents_ShouldReturnEmptyList()
    {
        _repo.GetHumanRobotInteractionEventItemsAsync(Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new List<HumanRobotInteractionEventItemDto>());

        var result = await Sut().Handle(new ListHumanRobotInteractionEventsQuery(TestTenantId.Value, null, FixedUtcNow.AddDays(-1), FixedUtcNow), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldReturnFailureWithRetrievalFailedErrorCode()
    {
        _repo.GetHumanRobotInteractionEventItemsAsync(Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyList<HumanRobotInteractionEventItemDto>>>(_ => throw new InvalidOperationException("db unavailable"));

        var result = await Sut().Handle(new ListHumanRobotInteractionEventsQuery(TestTenantId.Value, null, FixedUtcNow.AddDays(-1), FixedUtcNow), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("AutonomousVehicleSafety.HumanRobotInteractionEvent.RetrievalFailed");
    }
}

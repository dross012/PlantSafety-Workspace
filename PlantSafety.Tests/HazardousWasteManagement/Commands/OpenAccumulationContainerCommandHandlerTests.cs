// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.HazardousWasteManagement.Commands
//  Layer:          Tests (Unit)
//  File:           OpenAccumulationContainerCommandHandlerTests.cs
//  Created:        2026-08-04
//  Author:         Testing Agent
//  SchemaVersion:  2026.2
//  Description:    Handler tests for OpenAccumulationContainerCommandHandler.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.HazardousWasteManagement.Commands;
using PlantSafety.Domain.HazardousWasteManagement.WasteContainers.Interfaces;
using Agg = PlantSafety.Domain.HazardousWasteManagement.WasteContainers.Aggregates.WasteContainer;

namespace PlantSafety.Tests.HazardousWasteManagement.Commands;

public sealed class OpenAccumulationContainerCommandHandlerTests : UnitTestBase
{
    private readonly IWasteContainerRepository _repo = Substitute.For<IWasteContainerRepository>();

    private OpenAccumulationContainerCommandHandler Sut() => new(_repo, NullLogger<OpenAccumulationContainerCommandHandler>());

    private static OpenAccumulationContainerCommand ValidCommand() => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: TestCorrelationId.Value,
        CausationId: TestCausationId.Value,
        TenantId: TestTenantId.Value,
        SiteId: null,
        AccumulationAreaId: Guid.NewGuid(),
        WasteStreamId: Guid.NewGuid(),
        ContainerLabel: "DRUM-042",
        CapacityGallons: 55m,
        RegulatoryTimeLimitDays: 90,
        PerformedByUserId: TestUserId.Value);

    [Fact]
    public async Task Handle_WithValidCommand_PersistsAggregateAndReturnsSuccess()
    {
        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Open");
        await _repo.Received(1).AddAsync(Arg.Any<Agg>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_PassesContainerLabelAndCapacityThroughToAggregate()
    {
        var command = ValidCommand();

        await Sut().Handle(command, CancellationToken.None);

        await _repo.Received(1).AddAsync(
            Arg.Is<Agg>(a => a.ContainerLabel == "DRUM-042" && a.CapacityGallons == 55m && a.RegulatoryTimeLimitDays == 90),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsFailureWithoutPropagating()
    {
        _repo.AddAsync(Arg.Any<Agg>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException("SQL failure"));

        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("HazardousWasteManagement.WasteContainer.OpenFailed");
    }
}

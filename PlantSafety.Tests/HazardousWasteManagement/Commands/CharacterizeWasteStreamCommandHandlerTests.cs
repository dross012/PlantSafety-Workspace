// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.HazardousWasteManagement.Commands
//  Layer:          Tests (Unit)
//  File:           CharacterizeWasteStreamCommandHandlerTests.cs
//  Created:        2026-08-04
//  Author:         Testing Agent
//  SchemaVersion:  2026.2
//  Description:    Handler tests for CharacterizeWasteStreamCommandHandler.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.HazardousWasteManagement.Commands;
using PlantSafety.Domain.HazardousWasteManagement.WasteStreams.Interfaces;
using Agg = PlantSafety.Domain.HazardousWasteManagement.WasteStreams.Aggregates.WasteStream;

namespace PlantSafety.Tests.HazardousWasteManagement.Commands;

public sealed class CharacterizeWasteStreamCommandHandlerTests : UnitTestBase
{
    private readonly IWasteStreamRepository _repo = Substitute.For<IWasteStreamRepository>();

    private CharacterizeWasteStreamCommandHandler Sut() => new(_repo, NullLogger<CharacterizeWasteStreamCommandHandler>());

    private static CharacterizeWasteStreamCommand ValidCommand() => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: TestCorrelationId.Value,
        CausationId: TestCausationId.Value,
        TenantId: TestTenantId.Value,
        SiteId: null,
        StreamName: "Waste Oil Stream",
        EpaWasteCode: "D001",
        HazardClass: "Ignitable",
        GeneratorEpaId: "EPA123456789",
        PerformedByUserId: TestUserId.Value);

    [Fact]
    public async Task Handle_WithValidCommand_PersistsAggregateAndReturnsSuccess()
    {
        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Characterized");
        await _repo.Received(1).AddAsync(Arg.Any<Agg>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithUnknownHazardClass_DefaultsToNonHazardous()
    {
        var command = ValidCommand() with { HazardClass = "NotARealClass" };

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).AddAsync(
            Arg.Is<Agg>(a => a.HazardClass == PlantSafety.Domain.HazardousWasteManagement.WasteStreams.Enums.WasteHazardClass.NonHazardous),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsFailureWithoutPropagating()
    {
        _repo.AddAsync(Arg.Any<Agg>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException("SQL failure"));

        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("HazardousWasteManagement.WasteStream.CharacterizeFailed");
    }
}

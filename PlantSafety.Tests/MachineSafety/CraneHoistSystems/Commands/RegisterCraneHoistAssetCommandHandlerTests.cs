// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.MachineSafety.CraneHoistSystems.Commands
//  Layer:          Tests (Unit)
//  File:           RegisterCraneHoistAssetCommandHandlerTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Unit tests for RegisterCraneHoistAssetCommandHandler.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.MachineSafety.Commands;
using PlantSafety.Domain.MachineSafety.CraneHoistSystems.Interfaces;
using Agg = PlantSafety.Domain.MachineSafety.CraneHoistSystems.Aggregates.CraneHoistSystems;

namespace PlantSafety.Tests.MachineSafety.CraneHoistSystems.Commands;

public sealed class RegisterCraneHoistAssetCommandHandlerTests : UnitTestBase
{
    private readonly ICraneHoistSystemsRepository _repo = Substitute.For<ICraneHoistSystemsRepository>();

    private RegisterCraneHoistAssetCommandHandler Sut() => new(_repo, NullLogger<RegisterCraneHoistAssetCommandHandler>());

    private static RegisterCraneHoistAssetCommand ValidCommand() => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: TestCorrelationId.Value,
        CausationId: TestCausationId.Value,
        TenantId: TestTenantId.Value,
        SiteId: null,
        AssetTag: "CRANE-100",
        AssetTitle: "Main Bay Overhead Crane",
        AssetType: "OverheadBridgeCrane",
        RatedLoadCapacityKg: 5000m,
        PerformedByUserId: TestUserId.Value);

    [Fact]
    public async Task Handle_WithValidCommand_RegistersAssetAndReturnsSuccess()
    {
        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Registered");
        await _repo.Received(1).AddAsync(Arg.Any<Agg>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidAssetType_ReturnsFailure()
    {
        var command = ValidCommand() with { AssetType = "NotARealCraneType" };

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("MachineSafety.CraneHoistSystems.InvalidAssetType");
        await _repo.DidNotReceive().AddAsync(Arg.Any<Agg>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsFailureRatherThanPropagating()
    {
        _repo.AddAsync(Arg.Any<Agg>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException("db unavailable"));

        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("MachineSafety.CraneHoistSystems.RegisterFailed");
    }
}

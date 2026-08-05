// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.CyberPhysicalSecurity.Commands
//  Layer:          Tests (Unit)
//  File:           OtAssetCommandHandlerTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Testing Agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for RegisterOtAssetCommandHandler.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.CyberPhysicalSecurity.OtAsset.Commands;
using PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.Interfaces;
using OtAssetAggregate = PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.Aggregates.OtAsset;

namespace PlantSafety.Tests.CyberPhysicalSecurity.Commands;

public sealed class RegisterOtAssetCommandHandlerTests : UnitTestBase
{
    private readonly IOtAssetRepository _repo = Substitute.For<IOtAssetRepository>();

    private RegisterOtAssetCommandHandler Sut() => new(_repo, NullLogger<RegisterOtAssetCommandHandler>());

    private static RegisterOtAssetCommand ValidCommand() => new(
        Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), FixedUtcNow, TestTenantId.Value, Guid.NewGuid(),
        "PLC-101", "Line 3 PLC", "Plc", "Siemens", "S7-1500", "V2.9", "Level 1", "Medium", TestUserId.Value);

    [Fact]
    public async Task Handle_WithValidCommand_PersistsAggregateAndReturnsSuccess()
    {
        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Active");
        await _repo.Received(1).AddAsync(Arg.Any<OtAssetAggregate>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithUnrecognizedAssetType_DefaultsToOther()
    {
        var command = ValidCommand() with { AssetType = "NotARealType" };
        OtAssetAggregate? captured = null;
        await _repo.AddAsync(Arg.Do<OtAssetAggregate>(a => captured = a), Arg.Any<CancellationToken>());

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        captured.Should().NotBeNull();
        captured!.AssetType.ToString().Should().Be("Other");
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsFailureInsteadOfPropagating()
    {
        _repo.AddAsync(Arg.Any<OtAssetAggregate>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException("simulated DB failure"));

        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("CyberPhysicalSecurity.OtAsset.RegisterFailed");
    }
}

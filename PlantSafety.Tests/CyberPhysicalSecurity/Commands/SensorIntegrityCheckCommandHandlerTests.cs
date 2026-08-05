// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.CyberPhysicalSecurity.Commands
//  Layer:          Tests (Unit)
//  File:           SensorIntegrityCheckCommandHandlerTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Testing Agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for RunSensorIntegrityCheckCommandHandler.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.CyberPhysicalSecurity.SensorIntegrityCheck.Commands;
using PlantSafety.Domain.CyberPhysicalSecurity.SensorIntegrityCheck.Interfaces;
using SensorIntegrityCheckAggregate = PlantSafety.Domain.CyberPhysicalSecurity.SensorIntegrityCheck.Aggregates.SensorIntegrityCheck;

namespace PlantSafety.Tests.CyberPhysicalSecurity.Commands;

public sealed class RunSensorIntegrityCheckCommandHandlerTests : UnitTestBase
{
    private readonly ISensorIntegrityCheckRepository _repo = Substitute.For<ISensorIntegrityCheckRepository>();

    private RunSensorIntegrityCheckCommandHandler Sut() => new(_repo, NullLogger<RunSensorIntegrityCheckCommandHandler>());

    private static RunSensorIntegrityCheckCommand ValidCommand() => new(
        Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), FixedUtcNow, TestTenantId.Value, Guid.NewGuid(), Guid.NewGuid(),
        "Calibration", "Pass", "Within tolerance", TestUserId.Value);

    [Fact]
    public async Task Handle_WithValidCommand_PersistsAggregateAndReturnsSuccess()
    {
        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Result.Should().Be("Pass");
        await _repo.Received(1).AddAsync(Arg.Any<SensorIntegrityCheckAggregate>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithFailResult_PersistsFailedCheck()
    {
        var result = await Sut().Handle(ValidCommand() with { Result = "Fail" }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Result.Should().Be("Fail");
    }

    [Fact]
    public async Task Handle_WithUnrecognizedResult_DefaultsToInconclusive()
    {
        var result = await Sut().Handle(ValidCommand() with { Result = "NotARealResult" }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Result.Should().Be("Inconclusive");
    }

    [Fact]
    public async Task Handle_ComputesNextCheckDueUtc_ViaSensorIntegrityRules()
    {
        SensorIntegrityCheckAggregate? captured = null;
        await _repo.AddAsync(Arg.Do<SensorIntegrityCheckAggregate>(a => captured = a), Arg.Any<CancellationToken>());

        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        captured.Should().NotBeNull();
        captured!.NextCheckDueUtc.Should().NotBeNull("the recurring check-due scheduling hook must always compute a future due date");
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsFailureInsteadOfPropagating()
    {
        _repo.AddAsync(Arg.Any<SensorIntegrityCheckAggregate>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException("simulated DB failure"));

        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("CyberPhysicalSecurity.SensorIntegrityCheck.RunFailed");
    }
}

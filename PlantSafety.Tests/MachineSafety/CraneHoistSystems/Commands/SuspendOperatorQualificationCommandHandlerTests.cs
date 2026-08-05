// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.MachineSafety.CraneHoistSystems.Commands
//  Layer:          Tests (Unit)
//  File:           SuspendOperatorQualificationCommandHandlerTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Unit tests for SuspendOperatorQualificationCommandHandler.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.MachineSafety.Commands;
using PlantSafety.Domain.MachineSafety.CraneHoistSystems.Enums;
using PlantSafety.Domain.MachineSafety.CraneHoistSystems.Interfaces;
using PlantSafety.Domain.MachineSafety.CraneHoistSystems.ValueObjects.Ids;
using Agg = PlantSafety.Domain.MachineSafety.CraneHoistSystems.Aggregates.CraneHoistSystems;

namespace PlantSafety.Tests.MachineSafety.CraneHoistSystems.Commands;

public sealed class SuspendOperatorQualificationCommandHandlerTests : UnitTestBase
{
    private readonly ICraneHoistSystemsRepository _repo = Substitute.For<ICraneHoistSystemsRepository>();

    private SuspendOperatorQualificationCommandHandler Sut() => new(_repo, NullLogger<SuspendOperatorQualificationCommandHandler>());

    private static Agg QualifiedAsset()
    {
        var asset = Agg.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            TestTenantId.Value, null, "CRANE-7", "Crane Seven", CraneHoistAssetType.Hoist, 1500m);
        asset.QualifyOperator(TestUserId, FixedTimestamp, null, TestActor, TestCorrelationId, TestCausationId);
        return asset;
    }

    [Fact]
    public async Task Handle_WhenAssetExists_SuspendsQualificationAndReturnsSuccess()
    {
        var asset = QualifiedAsset();
        _repo.GetByIdAsync(Arg.Any<CraneHoistSystemsId>(), Arg.Any<CancellationToken>()).Returns(asset);

        var command = new SuspendOperatorQualificationCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            asset.CraneHoistSystemsId.Value, "failed re-certification", TestUserId.Value);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        asset.OperatorQualificationSuspended.Should().BeTrue();
        await _repo.Received(1).UpdateAsync(asset, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenAssetNotFound_ReturnsFailureWithNotFoundSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<CraneHoistSystemsId>(), Arg.Any<CancellationToken>()).Returns((Agg?)null);

        var command = new SuspendOperatorQualificationCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            Guid.NewGuid(), "reason", TestUserId.Value);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("MachineSafety.CraneHoistSystems.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsFailureRatherThanPropagating()
    {
        var asset = QualifiedAsset();
        _repo.GetByIdAsync(Arg.Any<CraneHoistSystemsId>(), Arg.Any<CancellationToken>()).Returns(asset);
        _repo.UpdateAsync(Arg.Any<Agg>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException("db unavailable"));

        var command = new SuspendOperatorQualificationCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            asset.CraneHoistSystemsId.Value, "reason", TestUserId.Value);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("MachineSafety.CraneHoistSystems.SuspendOperatorQualificationFailed");
    }
}

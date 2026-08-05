// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.MachineSafety.CraneHoistSystems.Commands
//  Layer:          Tests (Unit)
//  File:           RecordRiggingInspectionCommandHandlerTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Unit tests for RecordRiggingInspectionCommandHandler.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.MachineSafety.Commands;
using PlantSafety.Domain.MachineSafety.CraneHoistSystems.Enums;
using PlantSafety.Domain.MachineSafety.CraneHoistSystems.Interfaces;
using PlantSafety.Domain.MachineSafety.CraneHoistSystems.ValueObjects.Ids;
using Agg = PlantSafety.Domain.MachineSafety.CraneHoistSystems.Aggregates.CraneHoistSystems;

namespace PlantSafety.Tests.MachineSafety.CraneHoistSystems.Commands;

public sealed class RecordRiggingInspectionCommandHandlerTests : UnitTestBase
{
    private readonly ICraneHoistSystemsRepository _repo = Substitute.For<ICraneHoistSystemsRepository>();

    private RecordRiggingInspectionCommandHandler Sut() => new(_repo, NullLogger<RecordRiggingInspectionCommandHandler>());

    private static Agg RegisteredAsset() => Agg.Create(
        TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
        TestTenantId.Value, null, "CRANE-8", "Crane Eight", CraneHoistAssetType.OverheadBridgeCrane, 6000m);

    [Fact]
    public async Task Handle_WhenDefectDetected_RecordsInspectionAndReturnsSuccess()
    {
        var asset = RegisteredAsset();
        _repo.GetByIdAsync(Arg.Any<CraneHoistSystemsId>(), Arg.Any<CancellationToken>()).Returns(asset);

        var command = new RecordRiggingInspectionCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            asset.CraneHoistSystemsId.Value, true, "frayed sling detected", TestUserId.Value);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        asset.LastRiggingDefectDetected.Should().BeTrue();
        await _repo.Received(1).UpdateAsync(asset, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNullNotes_DoesNotThrow()
    {
        var asset = RegisteredAsset();
        _repo.GetByIdAsync(Arg.Any<CraneHoistSystemsId>(), Arg.Any<CancellationToken>()).Returns(asset);

        var command = new RecordRiggingInspectionCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            asset.CraneHoistSystemsId.Value, false, null, TestUserId.Value);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        asset.LastRiggingDefectDetected.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenAssetNotFound_ReturnsFailureWithNotFoundSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<CraneHoistSystemsId>(), Arg.Any<CancellationToken>()).Returns((Agg?)null);

        var command = new RecordRiggingInspectionCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            Guid.NewGuid(), false, null, TestUserId.Value);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("MachineSafety.CraneHoistSystems.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound);
    }

    [Fact]
    public async Task Handle_WhenAggregateThrows_ReturnsFailureRatherThanPropagating()
    {
        var asset = RegisteredAsset();
        asset.Retire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        _repo.GetByIdAsync(Arg.Any<CraneHoistSystemsId>(), Arg.Any<CancellationToken>()).Returns(asset);

        var command = new RecordRiggingInspectionCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            asset.CraneHoistSystemsId.Value, false, null, TestUserId.Value);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("MachineSafety.CraneHoistSystems.RecordRiggingInspectionFailed");
    }
}

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.LogisticsTransportSafety.LoadSecurementChecks
//  Layer:          Tests (Unit)
//  File:           LoadSecurementCheckCommandHandlerTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for all LoadSecurementCheck command handlers: Record,
//                  CompleteCorrectiveAction. Repository is mocked with NSubstitute; no I/O, no
//                  network, <50ms per test.
// ============================================================================
using PlantSafety.Application.LogisticsTransportSafety.Commands;
using PlantSafety.Domain.LogisticsTransportSafety.LoadSecurementChecks.Interfaces;
using PlantSafety.Domain.LogisticsTransportSafety.LoadSecurementChecks.ValueObjects.Ids;
using Agg = PlantSafety.Domain.LogisticsTransportSafety.LoadSecurementChecks.Aggregates.LoadSecurementCheck;

namespace PlantSafety.Tests.LogisticsTransportSafety.LoadSecurementChecks;

public sealed class LoadSecurementCheckCommandHandlerTests : UnitTestBase
{
    private static readonly Guid VehicleId = new("0000000f-0000-0000-0000-00000000000f");
    private static readonly Guid CheckedByUserId = new("00000010-0000-0000-0000-000000000010");
    private readonly ILoadSecurementCheckRepository _repo = Substitute.For<ILoadSecurementCheckRepository>();

    private static Agg Failed() =>
        Agg.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, vehicleId: VehicleId, checkedByUserId: CheckedByUserId,
            passed: false, deficiencyCount: 2, securementScore: 60m);

    // ---- RecordLoadSecurementCheckCommandHandler -----------------------------------

    [Fact]
    public async Task Record_WithPassingCommand_ShouldReturnSuccessAndCallAddAsync()
    {
        var sut = new RecordLoadSecurementCheckCommandHandler(_repo, NullLogger<RecordLoadSecurementCheckCommandHandler>());

        var result = await sut.Handle(new RecordLoadSecurementCheckCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null, null,
            VehicleId, CheckedByUserId, true, 0, 100m, TestUserId.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Passed");
        await _repo.Received(1).AddAsync(Arg.Any<Agg>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Record_WithFailingCommand_ShouldReturnFailedStatus()
    {
        var sut = new RecordLoadSecurementCheckCommandHandler(_repo, NullLogger<RecordLoadSecurementCheckCommandHandler>());

        var result = await sut.Handle(new RecordLoadSecurementCheckCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null, null,
            VehicleId, CheckedByUserId, false, 3, 55m, TestUserId.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Failed");
    }

    [Fact]
    public async Task Record_WithScoreOutOfRange_ShouldReturnGenericFailure()
    {
        var sut = new RecordLoadSecurementCheckCommandHandler(_repo, NullLogger<RecordLoadSecurementCheckCommandHandler>());

        var result = await sut.Handle(new RecordLoadSecurementCheckCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null, null,
            VehicleId, CheckedByUserId, true, 0, 150m, TestUserId.Value), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("LogisticsTransportSafety.LoadSecurementChecks.RecordFailed");
    }

    // ---- CompleteCorrectiveActionLoadSecurementCheckCommandHandler ------------------

    [Fact]
    public async Task CompleteCorrectiveAction_WhenFailed_ShouldReturnSuccess()
    {
        var check = Failed();
        _repo.GetByIdAsync(Arg.Any<LoadSecurementCheckId>(), Arg.Any<CancellationToken>()).Returns(check);
        var sut = new CompleteCorrectiveActionLoadSecurementCheckCommandHandler(_repo, NullLogger<CompleteCorrectiveActionLoadSecurementCheckCommandHandler>());

        var result = await sut.Handle(new CompleteCorrectiveActionLoadSecurementCheckCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            check.LoadSecurementCheckId.Value, TestUserId.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("CorrectiveActionCompleted");
        await _repo.Received(1).UpdateAsync(check, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CompleteCorrectiveAction_WhenAggregateNotFound_ShouldReturnFailure()
    {
        _repo.GetByIdAsync(Arg.Any<LoadSecurementCheckId>(), Arg.Any<CancellationToken>()).Returns((Agg?)null);
        var sut = new CompleteCorrectiveActionLoadSecurementCheckCommandHandler(_repo, NullLogger<CompleteCorrectiveActionLoadSecurementCheckCommandHandler>());

        var result = await sut.Handle(new CompleteCorrectiveActionLoadSecurementCheckCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            Guid.NewGuid(), TestUserId.Value), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("LogisticsTransportSafety.LoadSecurementChecks.NotFound");
    }

    [Fact]
    public async Task CompleteCorrectiveAction_WhenPassed_ShouldReturnGenericFailure()
    {
        var check = Agg.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, vehicleId: VehicleId, checkedByUserId: CheckedByUserId,
            passed: true, deficiencyCount: 0, securementScore: 100m);
        _repo.GetByIdAsync(Arg.Any<LoadSecurementCheckId>(), Arg.Any<CancellationToken>()).Returns(check);
        var sut = new CompleteCorrectiveActionLoadSecurementCheckCommandHandler(_repo, NullLogger<CompleteCorrectiveActionLoadSecurementCheckCommandHandler>());

        var result = await sut.Handle(new CompleteCorrectiveActionLoadSecurementCheckCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            check.LoadSecurementCheckId.Value, TestUserId.Value), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("LogisticsTransportSafety.LoadSecurementChecks.CompleteCorrectiveActionFailed");
    }
}

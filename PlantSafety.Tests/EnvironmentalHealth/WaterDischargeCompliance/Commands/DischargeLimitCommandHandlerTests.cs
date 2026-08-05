// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.EnvironmentalHealth.WaterDischargeCompliance.Commands
//  Layer:          Tests (Unit)
//  File:           DischargeLimitCommandHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Command handler tests for the DischargeLimit aggregate: Define, Revise, Retire.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.Commands;
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit.Enums;
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit.Interfaces;
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit.ValueObjects.Ids;
using Agg = PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit.Aggregates.DischargeLimit;

namespace PlantSafety.Tests.EnvironmentalHealth.WaterDischargeCompliance.Commands;

public sealed class DischargeLimitCommandHandlerTests : UnitTestBase
{
    private readonly IDischargeLimitRepository _repo = Substitute.For<IDischargeLimitRepository>();

    private static Agg DefinedLimit() => Agg.Create(
        createdBy: TestUserId, createdAtUtc: FixedTimestamp, correlationId: TestCorrelationId,
        causationId: TestCausationId, actor: TestActor, tenantId: TestTenantId.Value,
        dischargePointId: Guid.NewGuid(), dischargePermitId: Guid.NewGuid(),
        parameter: DischargeParameterType.PH, limitBasis: LimitBasisType.DailyMaximum,
        limitValue: 9.0m, unitOfMeasure: "pH", effectiveAtUtc: FixedTimestamp,
        permitConditionReference: "Part I.A.1");

    [Fact]
    public async Task Define_WithValidCommand_PersistsNewLimitAsActive()
    {
        var sut = new DefineDischargeLimitCommandHandler(_repo, NullLogger<DefineDischargeLimitCommandHandler>());
        var command = new DefineDischargeLimitCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, Guid.NewGuid(),
            Guid.NewGuid(), Guid.NewGuid(), "PH", "DailyMaximum", 9.0m, "pH", FixedUtcNow, "Part I.A.1", TestUserId.Value);

        var result = await sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(nameof(DischargeLimitStatus.Active));
        await _repo.Received(1).AddAsync(Arg.Any<Agg>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Revise_WhenActive_UpdatesValueAndPersists()
    {
        var limit = DefinedLimit();
        _repo.GetByIdAsync(Arg.Any<DischargeLimitId>(), Arg.Any<CancellationToken>()).Returns(limit);
        var sut = new ReviseDischargeLimitCommandHandler(_repo, NullLogger<ReviseDischargeLimitCommandHandler>());
        var command = new ReviseDischargeLimitCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            limit.DischargeLimitId.Value, 8.5m, TestUserId.Value);

        var result = await sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        limit.LimitValue.Should().Be(8.5m);
        await _repo.Received(1).UpdateAsync(limit, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Revise_WhenLimitDoesNotExist_ReturnsNotFoundFailure()
    {
        _repo.GetByIdAsync(Arg.Any<DischargeLimitId>(), Arg.Any<CancellationToken>()).Returns((Agg?)null);
        var sut = new ReviseDischargeLimitCommandHandler(_repo, NullLogger<ReviseDischargeLimitCommandHandler>());
        var command = new ReviseDischargeLimitCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            Guid.NewGuid(), 8.5m, TestUserId.Value);

        var result = await sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Contain("NotFound");
    }

    [Fact]
    public async Task Retire_WhenActive_TransitionsToRetiredAndPersists()
    {
        var limit = DefinedLimit();
        _repo.GetByIdAsync(Arg.Any<DischargeLimitId>(), Arg.Any<CancellationToken>()).Returns(limit);
        var sut = new RetireDischargeLimitCommandHandler(_repo, NullLogger<RetireDischargeLimitCommandHandler>());
        var command = new RetireDischargeLimitCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            limit.DischargeLimitId.Value, "Superseded.", TestUserId.Value);

        var result = await sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(nameof(DischargeLimitStatus.Retired));
    }

    [Fact]
    public async Task Revise_WhenAlreadyRetired_ReturnsFailureRatherThanThrowing()
    {
        var limit = DefinedLimit();
        limit.Retire(null, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        _repo.GetByIdAsync(Arg.Any<DischargeLimitId>(), Arg.Any<CancellationToken>()).Returns(limit);
        var sut = new ReviseDischargeLimitCommandHandler(_repo, NullLogger<ReviseDischargeLimitCommandHandler>());
        var command = new ReviseDischargeLimitCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            limit.DischargeLimitId.Value, 5.0m, TestUserId.Value);

        var result = await sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }
}

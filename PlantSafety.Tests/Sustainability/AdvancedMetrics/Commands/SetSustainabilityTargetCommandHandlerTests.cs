// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Sustainability.AdvancedMetrics.Commands
//  Layer:          Tests (Unit)
//  File:           SetSustainabilityTargetCommandHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Command handler tests for SetSustainabilityTargetCommandHandler:
//                  get-or-create ledger routing, invalid target type rejection, and the generic
//                  failure-envelope fallback. Isolated, deterministic, <50ms per test. No I/O, no
//                  network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Sustainability.AdvancedMetrics.Commands;
using PlantSafety.Domain.Sustainability.AdvancedMetrics.Interfaces;
using LedgerAggregate = PlantSafety.Domain.Sustainability.AdvancedMetrics.Aggregates.AdvancedMetricsLedger;

namespace PlantSafety.Tests.Sustainability.AdvancedMetrics.Commands;

public sealed class SetSustainabilityTargetCommandHandlerTests : UnitTestBase
{
    private readonly IAdvancedMetricsLedgerRepository _repo = Substitute.For<IAdvancedMetricsLedgerRepository>();

    private SetSustainabilityTargetCommandHandler Sut() => new(_repo, NullLogger<SetSustainabilityTargetCommandHandler>());

    private static readonly DateTime PeriodStart = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime PeriodEnd = new(2026, 3, 31, 0, 0, 0, DateTimeKind.Utc);

    private SetSustainabilityTargetCommand ValidCommand() => new(
        CommandId: Guid.NewGuid(), CorrelationId: TestCorrelationId.Value, CausationId: TestCausationId.Value,
        RequestedAtUtc: FixedUtcNow, ApiVersionToken: "v1", TenantId: TestTenantId.Value, SiteId: null,
        ReportingPeriodStartUtc: PeriodStart, ReportingPeriodEndUtc: PeriodEnd,
        TargetType: "WasteDiversionRate", BaselineValue: 40m, TargetValue: 75m,
        TargetDateUtc: FixedUtcNow.AddYears(1), RequesterUserId: TestUserId.Value);

    [Fact]
    public async Task Handle_WhenNoOpenLedgerExists_CreatesAndAddsNewLedger()
    {
        _repo.GetOpenLedgerAsync(Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<UtcTimestamp>(), Arg.Any<UtcTimestamp>(), Arg.Any<CancellationToken>())
            .Returns((LedgerAggregate?)null);

        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("OnTrack");
        await _repo.Received(1).AddAsync(Arg.Any<LedgerAggregate>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenOpenLedgerExists_UpdatesExistingLedgerInstead()
    {
        var existing = LedgerAggregate.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            TestTenantId.Value, null, UtcTimestamp.From(PeriodStart), UtcTimestamp.From(PeriodEnd));

        _repo.GetOpenLedgerAsync(Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<UtcTimestamp>(), Arg.Any<UtcTimestamp>(), Arg.Any<CancellationToken>())
            .Returns(existing);

        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).UpdateAsync(existing, Arg.Any<CancellationToken>());
        await _repo.DidNotReceive().AddAsync(Arg.Any<LedgerAggregate>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithUnrecognizedTargetType_ReturnsValidationFailureWithoutTouchingRepository()
    {
        var command = ValidCommand() with { TargetType = "NotARealTarget" };

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Sustainability.AdvancedMetrics.InvalidTargetType");
        result.Error.Severity.Should().Be(ErrorSeverity.Validation);
        await _repo.DidNotReceiveWithAnyArgs().GetOpenLedgerAsync(default, default, default!, default!, default);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsGenericFailureNotRawException()
    {
        _repo.GetOpenLedgerAsync(Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<UtcTimestamp>(), Arg.Any<UtcTimestamp>(), Arg.Any<CancellationToken>())
            .Returns<Task<LedgerAggregate?>>(_ => throw new InvalidOperationException("simulated repository outage"));

        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Sustainability.AdvancedMetrics.SetSustainabilityTargetFailed");
    }
}

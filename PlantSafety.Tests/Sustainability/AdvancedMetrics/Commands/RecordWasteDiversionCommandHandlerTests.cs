// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Sustainability.AdvancedMetrics.Commands
//  Layer:          Tests (Unit)
//  File:           RecordWasteDiversionCommandHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Command handler tests for RecordWasteDiversionCommandHandler: get-or-create
//                  ledger routing and the generic failure-envelope fallback. Isolated,
//                  deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Sustainability.AdvancedMetrics.Commands;
using PlantSafety.Domain.Sustainability.AdvancedMetrics.Interfaces;
using LedgerAggregate = PlantSafety.Domain.Sustainability.AdvancedMetrics.Aggregates.AdvancedMetricsLedger;

namespace PlantSafety.Tests.Sustainability.AdvancedMetrics.Commands;

public sealed class RecordWasteDiversionCommandHandlerTests : UnitTestBase
{
    private readonly IAdvancedMetricsLedgerRepository _repo = Substitute.For<IAdvancedMetricsLedgerRepository>();

    private RecordWasteDiversionCommandHandler Sut() => new(_repo, NullLogger<RecordWasteDiversionCommandHandler>());

    private static readonly DateTime PeriodStart = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime PeriodEnd = new(2026, 3, 31, 0, 0, 0, DateTimeKind.Utc);

    private RecordWasteDiversionCommand ValidCommand() => new(
        CommandId: Guid.NewGuid(), CorrelationId: TestCorrelationId.Value, CausationId: TestCausationId.Value,
        RequestedAtUtc: FixedUtcNow, ApiVersionToken: "v1", TenantId: TestTenantId.Value, SiteId: null,
        ReportingPeriodStartUtc: PeriodStart, ReportingPeriodEndUtc: PeriodEnd,
        TotalWasteKg: 1000m, DivertedWasteKg: 750m, MethodologyVersion: "Method-1",
        IntervalEndAtUtc: FixedUtcNow, RequesterUserId: TestUserId.Value);

    [Fact]
    public async Task Handle_WhenNoOpenLedgerExists_CreatesAndAddsNewLedgerWithComputedRate()
    {
        _repo.GetOpenLedgerAsync(Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<UtcTimestamp>(), Arg.Any<UtcTimestamp>(), Arg.Any<CancellationToken>())
            .Returns((LedgerAggregate?)null);

        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.DiversionRatePercent.Should().Be(75.00m);
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
    public async Task Handle_WhenRepositoryThrows_ReturnsGenericFailureNotRawException()
    {
        _repo.GetOpenLedgerAsync(Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<UtcTimestamp>(), Arg.Any<UtcTimestamp>(), Arg.Any<CancellationToken>())
            .Returns<Task<LedgerAggregate?>>(_ => throw new InvalidOperationException("simulated repository outage"));

        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Sustainability.AdvancedMetrics.RecordWasteDiversionFailed");
    }
}

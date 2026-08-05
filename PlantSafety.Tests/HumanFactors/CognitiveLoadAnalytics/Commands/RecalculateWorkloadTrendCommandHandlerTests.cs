// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.HumanFactors.CognitiveLoadAnalytics.Commands
//  Layer:          Tests (Unit)
//  File:           RecalculateWorkloadTrendCommandHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Unit tests for RecalculateWorkloadTrendCommandHandler: success path,
//                   not-found mapping, and tenant isolation. Isolated, deterministic,
//                   <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Commands;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Aggregates;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Enums;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Interfaces;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.ValueObjects.Ids;

namespace PlantSafety.Tests.HumanFactors.CognitiveLoadAnalytics.Commands;

public sealed class RecalculateWorkloadTrendCommandHandlerTests : UnitTestBase
{
    private readonly ICognitiveLoadLedgerRepository _repo = Substitute.For<ICognitiveLoadLedgerRepository>();

    private RecalculateWorkloadTrendCommandHandler Sut() => new(_repo, NullLogger<RecalculateWorkloadTrendCommandHandler>());

    private static CognitiveLoadLedger CreatedLedgerWithReading()
    {
        var ledger = CognitiveLoadLedger.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            TestTenantId.Value, null, ShiftType.Day, FixedTimestamp);
        ledger.RecordCognitiveLoadReading(TestUserId, null, 50m, "SelfReport", FixedTimestamp, FixedTimestamp, TestUserId, TestActor, TestCorrelationId, TestCausationId);
        return ledger;
    }

    private static RecalculateWorkloadTrendCommand CommandFor(Guid ledgerId) =>
        RecalculateWorkloadTrendCommand.FromDto(
            tenantId: TestTenantId.Value, cognitiveLoadLedgerId: ledgerId,
            periodStartUtc: FixedUtcNow.AddHours(-1), periodEndUtc: FixedUtcNow.AddHours(1),
            requesterUserId: TestUserId.Value, correlationId: TestCorrelationId.Value, causationId: TestCausationId.Value);

    [Fact]
    public async Task Handle_WithValidCommand_RecalculatesAndPersists()
    {
        var ledger = CreatedLedgerWithReading();
        _repo.GetByIdAsync(ledger.CognitiveLoadLedgerId, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(ledger);

        var result = await Sut().Handle(CommandFor(ledger.CognitiveLoadLedgerId.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ReadingCount.Should().Be(1);
        result.Value.MeanCognitiveLoadScore.Should().Be(50m);
        await _repo.Received(1).UpdateAsync(ledger, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenLedgerDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<CognitiveLoadLedgerId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((CognitiveLoadLedger?)null);

        var result = await Sut().Handle(CommandFor(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("HumanFactors.CognitiveLoadAnalytics.CognitiveLoadLedger.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound);
    }

    [Fact]
    public async Task Handle_WhenLedgerBelongsToDifferentTenant_ReturnsNotFound()
    {
        var ledgerId = CognitiveLoadLedgerId.New();
        var otherTenantId = Guid.NewGuid();
        _repo.GetByIdAsync(ledgerId, otherTenantId, Arg.Any<CancellationToken>()).Returns(CreatedLedgerWithReading());

        var result = await Sut().Handle(CommandFor(ledgerId.Value), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("HumanFactors.CognitiveLoadAnalytics.CognitiveLoadLedger.NotFound");
    }
}

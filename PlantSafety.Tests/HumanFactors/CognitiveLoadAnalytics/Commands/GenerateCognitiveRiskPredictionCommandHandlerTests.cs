// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.HumanFactors.CognitiveLoadAnalytics.Commands
//  Layer:          Tests (Unit)
//  File:           GenerateCognitiveRiskPredictionCommandHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GenerateCognitiveRiskPredictionCommandHandler: success path
//                   through the real CognitiveLoadAnalyticsService, not-found mapping, and
//                   unexpected exception mapping. Isolated, deterministic, <50ms per test.
//                   No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Commands;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Aggregates;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Enums;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Interfaces;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Services;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.ValueObjects.Ids;

namespace PlantSafety.Tests.HumanFactors.CognitiveLoadAnalytics.Commands;

public sealed class GenerateCognitiveRiskPredictionCommandHandlerTests : UnitTestBase
{
    private readonly ICognitiveLoadLedgerRepository _ledgerRepo = Substitute.For<ICognitiveLoadLedgerRepository>();
    private readonly ICognitiveRiskPredictionRepository _predictionRepo = Substitute.For<ICognitiveRiskPredictionRepository>();
    private readonly CognitiveLoadAnalyticsService _analyticsService = new();

    private GenerateCognitiveRiskPredictionCommandHandler Sut() =>
        new(_ledgerRepo, _predictionRepo, _analyticsService, NullLogger<GenerateCognitiveRiskPredictionCommandHandler>());

    private static CognitiveLoadLedger LedgerWithOverloadReadings()
    {
        var ledger = CognitiveLoadLedger.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            TestTenantId.Value, null, ShiftType.Day, FixedTimestamp);
        for (var i = 0; i < 4; i++)
            ledger.RecordCognitiveLoadReading(TestUserId, null, 90m, "SelfReport", FixedTimestamp, FixedTimestamp, TestUserId, TestActor, TestCorrelationId, TestCausationId);
        return ledger;
    }

    private static GenerateCognitiveRiskPredictionCommand CommandFor(Guid ledgerId) =>
        GenerateCognitiveRiskPredictionCommand.FromDto(
            tenantId: TestTenantId.Value, siteId: null, workerUserId: TestUserId.Value, sourceLedgerId: ledgerId,
            modelVersion: "v1.0.0", requesterUserId: TestUserId.Value,
            correlationId: TestCorrelationId.Value, causationId: TestCausationId.Value);

    [Fact]
    public async Task Handle_WithOverloadedLedger_GeneratesCriticalPredictionAndPersists()
    {
        var ledger = LedgerWithOverloadReadings();
        _ledgerRepo.GetByIdAsync(ledger.CognitiveLoadLedgerId, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(ledger);

        var result = await Sut().Handle(CommandFor(ledger.CognitiveLoadLedgerId.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.PredictedRiskLevel.Should().Be(nameof(CognitiveRiskLevel.Critical));
        result.Value.Status.Should().Be(nameof(CognitiveRiskPredictionStatus.Generated));
        await _predictionRepo.Received(1).AddAsync(Arg.Any<CognitiveRiskPrediction>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSourceLedgerDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _ledgerRepo.GetByIdAsync(Arg.Any<CognitiveLoadLedgerId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((CognitiveLoadLedger?)null);

        var result = await Sut().Handle(CommandFor(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("HumanFactors.CognitiveLoadAnalytics.CognitiveLoadLedger.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound);
        await _predictionRepo.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    public async Task Handle_WhenPredictionRepositoryThrows_ReturnsGenericFailureWithoutLeakingException()
    {
        var ledger = LedgerWithOverloadReadings();
        _ledgerRepo.GetByIdAsync(ledger.CognitiveLoadLedgerId, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(ledger);
        _predictionRepo.AddAsync(Arg.Any<CognitiveRiskPrediction>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException("boom"));

        var result = await Sut().Handle(CommandFor(ledger.CognitiveLoadLedgerId.Value), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("HumanFactors.CognitiveLoadAnalytics.GenerateCognitiveRiskPredictionFailed");
        result.Error.Message.Should().NotContain("boom");
    }
}

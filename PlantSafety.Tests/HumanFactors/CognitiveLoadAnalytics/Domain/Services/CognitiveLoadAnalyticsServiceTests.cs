// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.HumanFactors.CognitiveLoadAnalytics.Domain.Services
//  Layer:          Tests (Unit)
//  File:           CognitiveLoadAnalyticsServiceTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Tests for the CognitiveLoadAnalyticsService domain service: mean-load and
//                   overload-frequency computation from a ledger's readings, and the
//                   policy-delegating risk-level/prediction-score evaluation methods.
//                   Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Aggregates;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Enums;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Services;

namespace PlantSafety.Tests.HumanFactors.CognitiveLoadAnalytics.Domain.Services;

public sealed class CognitiveLoadAnalyticsServiceTests : UnitTestBase
{
    private readonly CognitiveLoadAnalyticsService _sut = new();

    private static CognitiveLoadLedger CreateOpenLedger() =>
        CognitiveLoadLedger.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            TestTenantId.Value, Guid.NewGuid(), ShiftType.Day, FixedTimestamp);

    [Fact]
    public void ComputeMeanCognitiveLoad_WithNoReadings_ReturnsNull()
    {
        var ledger = CreateOpenLedger();

        _sut.ComputeMeanCognitiveLoad(ledger).Should().BeNull();
    }

    [Fact]
    public void ComputeMeanCognitiveLoad_WithReadings_ReturnsRoundedAverage()
    {
        var ledger = CreateOpenLedger();
        ledger.RecordCognitiveLoadReading(TestUserId, null, 40m, "SelfReport", FixedTimestamp, FixedTimestamp, TestUserId, TestActor, TestCorrelationId, TestCausationId);
        ledger.RecordCognitiveLoadReading(TestUserId, null, 41m, "SelfReport", FixedTimestamp, FixedTimestamp, TestUserId, TestActor, TestCorrelationId, TestCausationId);

        _sut.ComputeMeanCognitiveLoad(ledger).Should().Be(40.5m);
    }

    [Fact]
    public void ComputeOverloadEventFrequencyPercent_WithNoReadings_ReturnsZero()
    {
        var ledger = CreateOpenLedger();

        _sut.ComputeOverloadEventFrequencyPercent(ledger).Should().Be(0m);
    }

    [Fact]
    public void ComputeOverloadEventFrequencyPercent_WithMixedReadings_ReturnsPercentOfOverloadReadings()
    {
        var ledger = CreateOpenLedger();
        ledger.RecordCognitiveLoadReading(TestUserId, null, 90m, "SelfReport", FixedTimestamp, FixedTimestamp, TestUserId, TestActor, TestCorrelationId, TestCausationId);
        ledger.RecordCognitiveLoadReading(TestUserId, null, 10m, "SelfReport", FixedTimestamp, FixedTimestamp, TestUserId, TestActor, TestCorrelationId, TestCausationId);
        ledger.RecordCognitiveLoadReading(TestUserId, null, 10m, "SelfReport", FixedTimestamp, FixedTimestamp, TestUserId, TestActor, TestCorrelationId, TestCausationId);
        ledger.RecordCognitiveLoadReading(TestUserId, null, 10m, "SelfReport", FixedTimestamp, FixedTimestamp, TestUserId, TestActor, TestCorrelationId, TestCausationId);

        _sut.ComputeOverloadEventFrequencyPercent(ledger).Should().Be(25m);
    }

    [Fact]
    public void EvaluatePredictedRiskLevel_DelegatesToCognitiveRiskPredictionPolicy()
    {
        var ledger = CreateOpenLedger();
        for (var i = 0; i < 4; i++)
            ledger.RecordCognitiveLoadReading(TestUserId, null, 90m, "SelfReport", FixedTimestamp, FixedTimestamp, TestUserId, TestActor, TestCorrelationId, TestCausationId);

        _sut.EvaluatePredictedRiskLevel(ledger).Should().Be(CognitiveRiskLevel.Critical);
    }

    [Fact]
    public void EstimatePredictionScore_WithNoReadings_UsesZeroInputsAndReturnsZero()
    {
        var ledger = CreateOpenLedger();

        _sut.EstimatePredictionScore(ledger).Should().Be(0m);
    }
}

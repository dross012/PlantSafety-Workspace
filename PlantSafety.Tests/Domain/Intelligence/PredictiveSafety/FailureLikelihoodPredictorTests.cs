// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Domain.Intelligence.PredictiveSafety
//  Layer:          Tests (Unit)
//  File:           FailureLikelihoodPredictorTests.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for FailureLikelihoodPredictor: weighted composite likelihood,
//                  alert threshold crossing, and guard-clause behavior.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.Intelligence.PredictiveSafety.Predictors;

namespace PlantSafety.Tests.Domain.Intelligence.PredictiveSafety;

/// <summary>Unit tests for <see cref="FailureLikelihoodPredictor"/>.</summary>
public sealed class FailureLikelihoodPredictorTests : UnitTestBase
{
    private readonly FailureLikelihoodPredictor _sut = new();

    private static FailureLikelihoodPredictionInput MakeInput(
        int ageMonths = 0, double runHours = 0, double expectedInterval = 500,
        int historicalFailures = 0, double anomalyScore = 0.0) =>
        new()
        {
            EquipmentAgeMonths = ageMonths,
            RunHoursSinceLastPm = runHours,
            ExpectedPmIntervalHours = expectedInterval,
            HistoricalFailureCount90Days = historicalFailures,
            AnomalyScore = anomalyScore,
        };

    [Fact]
    public void Predict_WithAllFactorsAtBaseline_ShouldReturnZeroLikelihood()
    {
        var result = _sut.Predict(MakeInput(), FixedTimestamp);
        result.Output.FailureLikelihood.Should().Be(0.0);
    }

    [Fact]
    public void Predict_WithHighAnomalyScore_ShouldRaiseLikelihood()
    {
        var result = _sut.Predict(MakeInput(anomalyScore: 1.0), FixedTimestamp);
        result.Output.FailureLikelihood.Should().BeGreaterThan(0.0);
    }

    [Fact]
    public void Predict_WithSevereOverrunAgeHistoryAndAnomaly_ShouldCrossAlertThreshold()
    {
        var result = _sut.Predict(
            MakeInput(ageMonths: 240, runHours: 1000, expectedInterval: 500, historicalFailures: 5, anomalyScore: 1.0),
            FixedTimestamp);
        result.Output.FailureLikelihood.Should().BeGreaterThanOrEqualTo(FailureLikelihoodPredictor.AlertThreshold);
        result.Output.ShouldRaiseAlert.Should().BeTrue();
    }

    [Fact]
    public void Predict_BelowAlertThreshold_ShouldNotRaiseAlert()
    {
        var result = _sut.Predict(MakeInput(), FixedTimestamp);
        result.Output.ShouldRaiseAlert.Should().BeFalse();
    }

    [Fact]
    public void Predict_LikelihoodShouldNeverExceedOne()
    {
        var result = _sut.Predict(
            MakeInput(ageMonths: 1000, runHours: 100000, expectedInterval: 1, historicalFailures: 50, anomalyScore: 5.0),
            FixedTimestamp);
        result.Output.FailureLikelihood.Should().BeLessThanOrEqualTo(1.0);
    }

    [Fact]
    public void Predict_WithZeroExpectedPmInterval_ShouldThrowArgumentOutOfRange()
    {
        var act = () => _sut.Predict(MakeInput(expectedInterval: 0), FixedTimestamp);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Predict_WithNegativeEquipmentAge_ShouldThrowArgumentOutOfRange()
    {
        var act = () => _sut.Predict(MakeInput(ageMonths: -1), FixedTimestamp);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Predict_WithNullInput_ShouldThrowArgumentNull()
    {
        var act = () => _sut.Predict(null!, FixedTimestamp);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Predict_WithHistoricalFailures_ShouldHaveHigherConfidenceThanWithout()
    {
        var withFailures = _sut.Predict(MakeInput(historicalFailures: 3), FixedTimestamp).Confidence.Value;
        var withoutFailures = _sut.Predict(MakeInput(historicalFailures: 0), FixedTimestamp).Confidence.Value;
        withFailures.Should().BeGreaterThan(withoutFailures);
    }

    [Fact]
    public void Predict_ShouldPopulateModelNameAndVersion()
    {
        var result = _sut.Predict(MakeInput(), FixedTimestamp);
        result.ModelName.Should().Be("FailureLikelihoodPredictor");
        result.ModelVersion.Should().Be("2026.1-heuristic");
    }

    [Fact]
    public void Predict_CalledTwiceWithSameInput_ShouldBeDeterministic()
    {
        var input = MakeInput(ageMonths: 36, runHours: 400, historicalFailures: 2, anomalyScore: 0.3);
        _sut.Predict(input, FixedTimestamp).Output.FailureLikelihood
            .Should().Be(_sut.Predict(input, FixedTimestamp).Output.FailureLikelihood);
    }
}

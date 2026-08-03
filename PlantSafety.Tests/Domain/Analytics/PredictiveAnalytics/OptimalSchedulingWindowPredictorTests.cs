// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Domain.Analytics.PredictiveAnalytics
//  Layer:          Tests (Unit)
//  File:           OptimalSchedulingWindowPredictorTests.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for OptimalSchedulingWindowPredictor: best-candidate selection,
//                  deterministic tie-breaking, and guard-clause behavior.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.Analytics.PredictiveAnalytics.Predictors;

namespace PlantSafety.Tests.Domain.Analytics.PredictiveAnalytics;

/// <summary>Unit tests for <see cref="OptimalSchedulingWindowPredictor"/>.</summary>
public sealed class OptimalSchedulingWindowPredictorTests : UnitTestBase
{
    private readonly OptimalSchedulingWindowPredictor _sut = new();

    private static SchedulingWindowCandidate Candidate(
        DateTime startUtc, double utilization, double failureRate, double fatigue) =>
        new()
        {
            StartUtc = UtcTimestamp.From(startUtc),
            ProjectedUtilization = utilization,
            HistoricalFailureRate = failureRate,
            ShiftFatigueIndex = fatigue,
        };

    [Fact]
    public void Predict_WithSingleCandidate_ShouldRecommendIt()
    {
        var candidate = Candidate(FixedUtcNow, 0.1, 0.1, 0.1);
        var result = _sut.Predict(new OptimalSchedulingWindowPredictionInput { Candidates = [candidate] }, FixedTimestamp);
        result.Output.RecommendedStartUtc.Should().Be(candidate.StartUtc);
    }

    [Fact]
    public void Predict_ShouldRecommendCandidateWithLowestCompositePenalty()
    {
        var best = Candidate(FixedUtcNow, 0.1, 0.1, 0.1);
        var worst = Candidate(FixedUtcNow.AddHours(1), 0.9, 0.9, 0.9);
        var result = _sut.Predict(
            new OptimalSchedulingWindowPredictionInput { Candidates = [worst, best] }, FixedTimestamp);
        result.Output.RecommendedStartUtc.Should().Be(best.StartUtc);
    }

    [Fact]
    public void Predict_WithTiedScores_ShouldBreakTieByEarliestStart()
    {
        var earlier = Candidate(FixedUtcNow, 0.2, 0.2, 0.2);
        var later = Candidate(FixedUtcNow.AddHours(2), 0.2, 0.2, 0.2);
        var result = _sut.Predict(
            new OptimalSchedulingWindowPredictionInput { Candidates = [later, earlier] }, FixedTimestamp);
        result.Output.RecommendedStartUtc.Should().Be(earlier.StartUtc);
    }

    [Fact]
    public void Predict_ScoreShouldBeOneMinusPenaltyForZeroRiskCandidate()
    {
        var candidate = Candidate(FixedUtcNow, 0.0, 0.0, 0.0);
        var result = _sut.Predict(new OptimalSchedulingWindowPredictionInput { Candidates = [candidate] }, FixedTimestamp);
        result.Output.Score.Should().Be(1.0);
    }

    [Fact]
    public void Predict_WithEmptyCandidateList_ShouldThrowArgumentException()
    {
        var act = () => _sut.Predict(new OptimalSchedulingWindowPredictionInput { Candidates = [] }, FixedTimestamp);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Predict_WithNullInput_ShouldThrowArgumentNull()
    {
        var act = () => _sut.Predict(null!, FixedTimestamp);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Predict_ShouldPopulateModelNameAndVersion()
    {
        var candidate = Candidate(FixedUtcNow, 0.1, 0.1, 0.1);
        var result = _sut.Predict(new OptimalSchedulingWindowPredictionInput { Candidates = [candidate] }, FixedTimestamp);
        result.ModelName.Should().Be("OptimalSchedulingWindowPredictor");
        result.ModelVersion.Should().Be("2026.1-heuristic");
    }

    [Fact]
    public void Predict_ConfidenceShouldIncreaseWithMoreCandidates()
    {
        var few = _sut.Predict(new OptimalSchedulingWindowPredictionInput
        {
            Candidates = [Candidate(FixedUtcNow, 0.1, 0.1, 0.1)],
        }, FixedTimestamp).Confidence.Value;

        var many = _sut.Predict(new OptimalSchedulingWindowPredictionInput
        {
            Candidates = Enumerable.Range(0, 10)
                .Select(i => Candidate(FixedUtcNow.AddHours(i), 0.1, 0.1, 0.1)).ToList(),
        }, FixedTimestamp).Confidence.Value;

        many.Should().BeGreaterThan(few);
    }
}

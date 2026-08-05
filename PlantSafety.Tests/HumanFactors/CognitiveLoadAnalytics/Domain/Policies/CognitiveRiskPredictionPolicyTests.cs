// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.HumanFactors.CognitiveLoadAnalytics.Domain.Policies
//  Layer:          Tests (Unit)
//  File:           CognitiveRiskPredictionPolicyTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Tests for CognitiveRiskPredictionPolicy.EvaluateRiskLevel,
//                   EstimatePredictionScore, and IsAlertWorthy. Isolated, deterministic,
//                   <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Enums;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Policies;

namespace PlantSafety.Tests.HumanFactors.CognitiveLoadAnalytics.Domain.Policies;

public sealed class CognitiveRiskPredictionPolicyTests
{
    [Theory]
    [InlineData(10, 0, CognitiveRiskLevel.Low)]
    [InlineData(44.99, 9.99, CognitiveRiskLevel.Low)]
    [InlineData(45, 0, CognitiveRiskLevel.Moderate)]
    [InlineData(0, 10, CognitiveRiskLevel.Moderate)]
    [InlineData(64.99, 24.99, CognitiveRiskLevel.Moderate)]
    [InlineData(65, 0, CognitiveRiskLevel.Elevated)]
    [InlineData(0, 25, CognitiveRiskLevel.Elevated)]
    [InlineData(79.99, 39.99, CognitiveRiskLevel.Elevated)]
    [InlineData(80, 0, CognitiveRiskLevel.Critical)]
    [InlineData(0, 40, CognitiveRiskLevel.Critical)]
    public void EvaluateRiskLevel_AtBoundaries_ReturnsExpectedLevel(decimal meanLoad, decimal overloadFrequency, CognitiveRiskLevel expected)
    {
        CognitiveRiskPredictionPolicy.EvaluateRiskLevel(meanLoad, overloadFrequency).Should().Be(expected);
    }

    [Fact]
    public void EstimatePredictionScore_CombinesInputsWithWeightingAndRounds()
    {
        var score = CognitiveRiskPredictionPolicy.EstimatePredictionScore(70m, 30m);

        // (70 * 0.6 + 30 * 0.4) / 100 = (42 + 12) / 100 = 0.54
        score.Should().Be(0.54m);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(100, 100)]
    public void EstimatePredictionScore_IsClampedToUnitInterval(decimal meanLoad, decimal overloadFrequency)
    {
        var score = CognitiveRiskPredictionPolicy.EstimatePredictionScore(meanLoad, overloadFrequency);

        score.Should().BeInRange(0m, 1m);
    }

    [Theory]
    [InlineData(CognitiveRiskLevel.Low, false)]
    [InlineData(CognitiveRiskLevel.Moderate, false)]
    [InlineData(CognitiveRiskLevel.Elevated, true)]
    [InlineData(CognitiveRiskLevel.Critical, true)]
    public void IsAlertWorthy_ReturnsExpected(CognitiveRiskLevel level, bool expected)
    {
        CognitiveRiskPredictionPolicy.IsAlertWorthy(level).Should().Be(expected);
    }
}

// ============================================================================
//  Project:        AI.PredictiveOptimization.Tests
//  File:           PredictiveOptimizationEngineTests.cs
//  Created:        2026-08-08
//  Description:    Phase E.6 unit tests for PredictiveOptimizationEngine.Optimize:
//                  per-target forecast/weighted-score computation, anomaly
//                  detection, composite score summation, recommended-focus
//                  selection, and empty-input rejection.
// ============================================================================
using FluentAssertions;

using AI.PredictiveOptimization.Abstractions.Engine;
using AI.PredictiveOptimization.Abstractions.Models;
using AI.PredictiveOptimization.Abstractions.Optimization;

using Xunit;

namespace AI.PredictiveOptimization.Tests.Engine;

public sealed class PredictiveOptimizationEngineTests
{
    [Fact]
    public void Optimize_ComputesWeightedScorePerTarget()
    {
        var input = new PredictiveOptimizationInput(
            new Dictionary<OptimizationTarget, IReadOnlyList<double>> { [OptimizationTarget.HazardLikelihood] = [1, 2, 3, 4, 5] },
            new Dictionary<OptimizationTarget, double> { [OptimizationTarget.HazardLikelihood] = 2.0 },
            WindowSize: 3);

        PredictiveOptimizationOutput output = PredictiveOptimizationEngine.Optimize(input);

        PredictiveOptimizationModel model = output.Models.Should().ContainSingle().Subject;
        model.WeightedScore.Should().Be(model.ForecastedValue * 2.0);
    }

    [Fact]
    public void Optimize_MissingWeight_DefaultsToOne()
    {
        var input = new PredictiveOptimizationInput(
            new Dictionary<OptimizationTarget, IReadOnlyList<double>> { [OptimizationTarget.WorkflowEfficiency] = [5, 5, 5] },
            new Dictionary<OptimizationTarget, double>(),
            WindowSize: 3);

        PredictiveOptimizationOutput output = PredictiveOptimizationEngine.Optimize(input);

        output.Models.Single().Weight.Should().Be(1.0);
    }

    [Fact]
    public void Optimize_SpikeAtEnd_IsAnomalous()
    {
        var input = new PredictiveOptimizationInput(
            new Dictionary<OptimizationTarget, IReadOnlyList<double>> { [OptimizationTarget.MachineFailureLikelihood] = [0, 0, 0, 0, 50] },
            new Dictionary<OptimizationTarget, double> { [OptimizationTarget.MachineFailureLikelihood] = 1.0 },
            WindowSize: 4);

        PredictiveOptimizationOutput output = PredictiveOptimizationEngine.Optimize(input);

        output.Models.Single().IsAnomalous.Should().BeTrue();
    }

    [Fact]
    public void Optimize_RecommendsHighestWeightedScoreTarget()
    {
        var input = new PredictiveOptimizationInput(
            new Dictionary<OptimizationTarget, IReadOnlyList<double>>
            {
                [OptimizationTarget.HazardLikelihood] = [1, 1, 1],
                [OptimizationTarget.SeverityLikelihood] = [10, 10, 10],
            },
            new Dictionary<OptimizationTarget, double>
            {
                [OptimizationTarget.HazardLikelihood] = 1.0,
                [OptimizationTarget.SeverityLikelihood] = 1.0,
            },
            WindowSize: 3);

        PredictiveOptimizationOutput output = PredictiveOptimizationEngine.Optimize(input);

        output.RecommendedFocusTarget.Should().Be(OptimizationTarget.SeverityLikelihood);
    }

    [Fact]
    public void Optimize_CompositeScore_IsSumOfWeightedScores()
    {
        var input = new PredictiveOptimizationInput(
            new Dictionary<OptimizationTarget, IReadOnlyList<double>>
            {
                [OptimizationTarget.HazardLikelihood] = [2, 2, 2],
                [OptimizationTarget.ExposureLikelihood] = [3, 3, 3],
            },
            new Dictionary<OptimizationTarget, double>
            {
                [OptimizationTarget.HazardLikelihood] = 1.0,
                [OptimizationTarget.ExposureLikelihood] = 1.0,
            },
            WindowSize: 3);

        PredictiveOptimizationOutput output = PredictiveOptimizationEngine.Optimize(input);

        output.CompositeOptimizationScore.Should().Be(output.Models.Sum(m => m.WeightedScore));
    }

    [Fact]
    public void Optimize_EmptyHistory_Throws()
    {
        var input = new PredictiveOptimizationInput(new Dictionary<OptimizationTarget, IReadOnlyList<double>>(), new Dictionary<OptimizationTarget, double>(), WindowSize: 3);

        Action act = () => PredictiveOptimizationEngine.Optimize(input);

        act.Should().Throw<ArgumentException>();
    }
}

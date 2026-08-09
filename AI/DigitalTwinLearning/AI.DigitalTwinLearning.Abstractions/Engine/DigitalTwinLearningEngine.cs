// ============================================================================
//  Project:        AI.DigitalTwinLearning
//  Module:         Abstractions.Engine
//  File:           DigitalTwinLearningEngine.cs
//  Created:        2026-08-08
//  Description:    Phase E.7 (Path E: AI-Driven Safety Automation):
//                  deterministically learns each of the five pattern
//                  categories Path E's spec requires by running Shared.
//                  PredictiveModels' real DeterministicSeriesMath (Phase
//                  C.11, unchanged) against each category's occurrence
//                  history, then evaluates every DigitalTwinLearningRule
//                  against each learned pattern. Pure function, no side
//                  effects, no reflection.
// ============================================================================
using Shared.PredictiveModels.Abstractions.Statistics;

using AI.DigitalTwinLearning.Abstractions.Learning;
using AI.DigitalTwinLearning.Abstractions.Rules;

namespace AI.DigitalTwinLearning.Abstractions.Engine;

/// <summary>Deterministically learns hazard/severity/workflow/exposure/failure patterns from historical occurrence counts.</summary>
public static class DigitalTwinLearningEngine
{
    /// <summary>Learns every pattern category present in the input and evaluates every rule against each.</summary>
    public static DigitalTwinLearningOutput Learn(DigitalTwinLearningInput input, IReadOnlyList<DigitalTwinLearningRule> rules, DateTime? nowUtc = null)
    {
        var patterns = new List<LearnedPattern>
        {
            AnalyzePattern("HazardPropagation", input.HazardPropagationOccurrences, input.WindowSize),
            AnalyzePattern("SeverityEscalation", input.SeverityEscalationOccurrences, input.WindowSize),
            AnalyzePattern("WorkflowBottleneck", input.WorkflowBottleneckOccurrences, input.WindowSize),
            AnalyzePattern("EmployeeExposure", input.EmployeeExposureOccurrences, input.WindowSize),
            AnalyzePattern("MachineFailure", input.MachineFailureOccurrences, input.WindowSize),
        };

        IReadOnlyList<string> recommendations = patterns
            .SelectMany(pattern => rules.Where(rule => rule.Condition(pattern)))
            .Select(rule => rule.ModelUpdateRecommendation)
            .ToArray();

        return new DigitalTwinLearningOutput(patterns, recommendations, nowUtc ?? DateTime.UtcNow);
    }

    private static LearnedPattern AnalyzePattern(string patternName, IReadOnlyList<int> occurrences, int windowSize)
    {
        IReadOnlyList<double> series = occurrences.Select(c => (double)c).ToArray();
        double movingAverage = DeterministicSeriesMath.MovingAverage(series, windowSize);
        double trendSlope = DeterministicSeriesMath.TrendSlope(series);
        double zScore = series.Count > 1
            ? DeterministicSeriesMath.ZScore(series.Take(series.Count - 1).ToArray(), series[^1])
            : 0d;

        return new LearnedPattern(patternName, movingAverage, trendSlope, DeterministicSeriesMath.IsAnomalous(zScore));
    }
}

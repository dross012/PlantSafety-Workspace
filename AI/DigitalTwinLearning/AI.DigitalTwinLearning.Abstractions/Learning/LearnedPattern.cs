// ============================================================================
//  Project:        AI.DigitalTwinLearning
//  Module:         Abstractions.Learning
//  File:           LearnedPattern.cs
//  Created:        2026-08-08
//  Description:    Phase E.7: one deterministically learned pattern --
//                  moving average, OLS trend slope, and z-score anomaly
//                  flag for a single occurrence-count history, via Shared.
//                  PredictiveModels' DeterministicSeriesMath (Phase C.11).
//                  Not in Path E's original file list, but required to
//                  express DigitalTwinLearningOutput.Patterns explicitly
//                  (mirrors WorkflowDecision's Phase C.7 precedent).
// ============================================================================
namespace AI.DigitalTwinLearning.Abstractions.Learning;

/// <summary>One deterministically learned pattern from a historical occurrence-count series.</summary>
public sealed record LearnedPattern(
    string PatternName,
    double MovingAverage,
    double TrendSlope,
    bool IsAnomalous);

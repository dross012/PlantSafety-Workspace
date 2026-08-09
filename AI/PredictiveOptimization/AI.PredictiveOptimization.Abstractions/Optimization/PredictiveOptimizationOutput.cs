// ============================================================================
//  Project:        AI.PredictiveOptimization
//  Module:         Abstractions.Optimization
//  File:           PredictiveOptimizationOutput.cs
//  Created:        2026-08-08
//  Description:    Phase E.6: the result of a predictive-optimization
//                  session -- one PredictiveOptimizationModel per target,
//                  the composite (sum of weighted scores) score, and the
//                  rule-based recommended focus target (the one with the
//                  highest weighted score -- deterministic, no invented
//                  tie-break beyond "first by enum declaration order").
// ============================================================================
using AI.PredictiveOptimization.Abstractions.Models;

namespace AI.PredictiveOptimization.Abstractions.Optimization;

/// <summary>The result of a predictive-optimization session.</summary>
public sealed record PredictiveOptimizationOutput(
    IReadOnlyList<PredictiveOptimizationModel> Models,
    OptimizationTarget RecommendedFocusTarget,
    double CompositeOptimizationScore,
    DateTime GeneratedAtUtc);

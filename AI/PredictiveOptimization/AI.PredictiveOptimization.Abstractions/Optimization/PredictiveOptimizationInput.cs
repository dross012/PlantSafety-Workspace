// ============================================================================
//  Project:        AI.PredictiveOptimization
//  Module:         Abstractions.Optimization
//  File:           PredictiveOptimizationInput.cs
//  Created:        2026-08-08
//  Description:    Phase E.6: the input PredictiveOptimizationEngine.Optimize
//                  reasons over -- one historical time series per
//                  OptimizationTarget, a shared rolling-window size, and
//                  the deterministic weight to apply to each target's
//                  forecast when computing its weighted score.
// ============================================================================
using AI.PredictiveOptimization.Abstractions.Models;

namespace AI.PredictiveOptimization.Abstractions.Optimization;

/// <summary>The input a predictive-optimization session reasons over.</summary>
public sealed record PredictiveOptimizationInput(
    IReadOnlyDictionary<OptimizationTarget, IReadOnlyList<double>> HistoryByTarget,
    IReadOnlyDictionary<OptimizationTarget, double> WeightByTarget,
    int WindowSize);

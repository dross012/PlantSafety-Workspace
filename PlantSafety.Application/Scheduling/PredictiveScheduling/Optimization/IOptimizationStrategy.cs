// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: Contract for a pluggable predictive scheduling optimization strategy.
// SchemaVersion: 1

namespace PlantSafety.Application.Scheduling.PredictiveScheduling.Optimization;

/// <summary>A pluggable strategy that places candidate tasks onto resources for a given <see cref="OptimizationContext"/>.</summary>
public interface IOptimizationStrategy
{
    /// <summary>Stable machine-readable strategy name (e.g. "Greedy", "ConstraintSatisfaction", "Genetic").</summary>
    string StrategyName { get; }

    /// <summary>Produces a feasible schedule for the supplied <paramref name="context"/>.</summary>
    OptimizationResult Optimize(OptimizationContext context);
}

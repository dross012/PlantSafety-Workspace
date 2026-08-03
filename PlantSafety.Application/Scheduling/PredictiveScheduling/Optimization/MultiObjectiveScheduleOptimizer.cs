// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: Multi-objective optimization aggregator. Runs every registered
// IOptimizationStrategy against the same OptimizationContext and selects the result that
// best serves the context's OptimizationObjective, blending coverage, safety margin, and
// workload-balance signals into a single comparable objective score.
// SchemaVersion: 1

using PlantSafety.Domain.Scheduling.PredictiveScheduling.Enums;
using PlantSafety.Domain.Scheduling.PredictiveScheduling.ValueObjects;

namespace PlantSafety.Application.Scheduling.PredictiveScheduling.Optimization;

/// <summary>
/// Runs every registered <see cref="IOptimizationStrategy"/> against the same context and
/// selects (or blends) the result that best serves the requested <see cref="OptimizationObjective"/>.
/// </summary>
public sealed class MultiObjectiveScheduleOptimizer
{
    private readonly IReadOnlyList<IOptimizationStrategy> _strategies;

    /// <summary>Creates the optimizer with the platform-default strategy set (Greedy, ConstraintSatisfaction, Genetic).</summary>
    public MultiObjectiveScheduleOptimizer()
        : this([new GreedyOptimizationStrategy(), new ConstraintSatisfactionStrategy(), new GeneticAlgorithmOptimizationStrategy()]) { }

    /// <summary>Creates the optimizer with an explicit set of strategies (for testing/composition).</summary>
    public MultiObjectiveScheduleOptimizer(IReadOnlyList<IOptimizationStrategy> strategies)
    {
        ArgumentNullException.ThrowIfNull(strategies);
        if (strategies.Count == 0)
            throw new ArgumentException("At least one IOptimizationStrategy must be supplied.", nameof(strategies));
        _strategies = strategies;
    }

    /// <summary>
    /// Runs every configured strategy and returns the single best-scoring result for
    /// <paramref name="context"/>.Objective, along with every candidate result for observability.
    /// </summary>
    public MultiObjectiveOptimizationOutcome OptimizeBest(OptimizationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var candidates = _strategies
            .Select(strategy => (Strategy: strategy.StrategyName, Result: strategy.Optimize(context)))
            .Select(x => (x.Strategy, x.Result, ObjectiveScore: ScoreForObjective(x.Result, context)))
            .OrderByDescending(x => x.ObjectiveScore)
            .ToList();

        var winner = candidates[0];

        return new MultiObjectiveOptimizationOutcome
        {
            SelectedStrategyName = winner.Strategy,
            SelectedResult = winner.Result,
            AllCandidateResults = candidates.Select(c => c.Result).ToList(),
        };
    }

    /// <summary>
    /// Computes a comparable objective score for <paramref name="result"/> against
    /// <paramref name="context"/>.Objective, blending coverage, tolerated-violation penalty,
    /// safety-margin, and workload-balance signals.
    /// </summary>
    private static double ScoreForObjective(OptimizationResult result, OptimizationContext context)
    {
        var totalTasks = context.Tasks.Count;
        var coverage = totalTasks == 0 ? 1.0 : result.Assignments.Count / (double)totalTasks;
        var violationPenalty = result.ToleratedViolations.Count * 0.02;

        return context.Objective switch
        {
            OptimizationObjective.MaximizeCoverage => coverage - violationPenalty,

            OptimizationObjective.MinimizeMakespan => coverage - violationPenalty - MakespanPenalty(result),

            OptimizationObjective.MaximizeSafetyMargin => coverage - violationPenalty - SafetyPenalty(result, context),

            OptimizationObjective.BalanceWorkload => coverage - violationPenalty - WorkloadImbalancePenalty(result),

            _ => coverage - violationPenalty,
        };
    }

    private static double MakespanPenalty(OptimizationResult result)
    {
        if (result.Assignments.Count == 0)
            return 0.0;

        var makespan = result.Assignments.Max(a => a.EndUtc.Value) - result.Assignments.Min(a => a.StartUtc.Value);
        return Math.Min(0.3, makespan.TotalDays * 0.01);
    }

    private static double SafetyPenalty(OptimizationResult result, OptimizationContext context)
    {
        var safetyCriticalTaskIds = context.Tasks.Where(t => t.IsSafetyCritical).Select(t => t.TaskId).ToHashSet();
        if (safetyCriticalTaskIds.Count == 0)
            return 0.0;

        var unscheduledSafetyCritical = result.UnscheduledTaskIds.Count(id => safetyCriticalTaskIds.Contains(id));
        return unscheduledSafetyCritical / (double)safetyCriticalTaskIds.Count * 0.5;
    }

    private static double WorkloadImbalancePenalty(OptimizationResult result)
    {
        if (result.Assignments.Count == 0)
            return 0.0;

        var byResource = result.Assignments
            .GroupBy(a => a.ResourceId)
            .Select(g => g.Sum(a => (a.EndUtc.Value - a.StartUtc.Value).TotalHours))
            .ToList();

        if (byResource.Count < 2)
            return 0.0;

        var mean = byResource.Average();
        var variance = byResource.Sum(h => Math.Pow(h - mean, 2)) / byResource.Count;
        var stdDev = Math.Sqrt(variance);

        return Math.Min(0.3, stdDev / Math.Max(1.0, mean) * 0.1);
    }
}

/// <summary>Result of running every configured strategy through <see cref="MultiObjectiveScheduleOptimizer"/>.</summary>
public sealed record MultiObjectiveOptimizationOutcome
{
    /// <summary>Name of the strategy whose result was selected as best for the requested objective.</summary>
    public required string SelectedStrategyName { get; init; }

    /// <summary>The winning strategy's result.</summary>
    public required OptimizationResult SelectedResult { get; init; }

    /// <summary>Every candidate result produced, for observability/audit.</summary>
    public required IReadOnlyList<OptimizationResult> AllCandidateResults { get; init; }
}

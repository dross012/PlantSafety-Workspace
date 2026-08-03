// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: Immutable output envelope produced by an IOptimizationStrategy run.
// SchemaVersion: 1

using PlantSafety.Domain.Scheduling.PredictiveScheduling.ValueObjects;
using PlantSafety.Domain.Scheduling.PredictiveScheduling.ValueObjects.Ids;

namespace PlantSafety.Application.Scheduling.PredictiveScheduling.Optimization;

/// <summary>Immutable output envelope produced by a single <see cref="IOptimizationStrategy"/> run.</summary>
public sealed record OptimizationResult
{
    /// <summary>Stable machine-readable name of the strategy that produced this result.</summary>
    public required string StrategyName { get; init; }

    /// <summary>Feasible task-to-resource assignments produced by the strategy.</summary>
    public required IReadOnlyList<ScheduleAssignment> Assignments { get; init; }

    /// <summary>Tasks the strategy was unable to place within a feasible window.</summary>
    public required IReadOnlyList<SchedulableTaskId> UnscheduledTaskIds { get; init; }

    /// <summary>Non-blocking (Warning-severity) violations tolerated by the accepted assignments.</summary>
    public IReadOnlyList<ConstraintViolation> ToleratedViolations { get; init; } = [];

    /// <summary>Composite desirability score for this result (higher is better); comparable only across results for the same objective.</summary>
    public required double Score { get; init; }
}

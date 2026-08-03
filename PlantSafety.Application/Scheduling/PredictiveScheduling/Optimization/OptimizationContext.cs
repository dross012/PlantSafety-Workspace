// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: Immutable input envelope for a predictive scheduling optimization run:
// the candidate tasks, resource availability, already-committed assignments, and the
// objective the strategy should optimize for.
// SchemaVersion: 1

using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Scheduling.PredictiveScheduling.Enums;
using PlantSafety.Domain.Scheduling.PredictiveScheduling.ValueObjects;

namespace PlantSafety.Application.Scheduling.PredictiveScheduling.Optimization;

/// <summary>Immutable input envelope supplied to every <see cref="IOptimizationStrategy"/> run.</summary>
public sealed record OptimizationContext
{
    /// <summary>Tasks competing for placement in this optimization run.</summary>
    public required IReadOnlyList<SchedulableTask> Tasks { get; init; }

    /// <summary>Declared availability windows for every candidate resource (crew and machine).</summary>
    public required IReadOnlyList<ResourceAvailabilityWindow> Availability { get; init; }

    /// <summary>Assignments already committed prior to this optimization run (immovable).</summary>
    public IReadOnlyList<ScheduleAssignment> ExistingAssignments { get; init; } = [];

    /// <summary>Objective the strategy should optimize for.</summary>
    public OptimizationObjective Objective { get; init; } = OptimizationObjective.MaximizeCoverage;

    /// <summary>UTC instant the optimization run is being executed as of (used for priority weighting).</summary>
    public required UtcTimestamp AsOfUtc { get; init; }
}

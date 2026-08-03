// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: Greedy predictive scheduling optimization strategy. Orders tasks by
// composite priority weight (PriorityWeightingService) and, for each task in turn, commits
// it to the earliest feasible resource window found via ResourceAvailabilityModel and
// validated by ConstraintEvaluator. Fast, deterministic, O(n log n + n*m).
// SchemaVersion: 1

using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Scheduling.PredictiveScheduling.Services;
using PlantSafety.Domain.Scheduling.PredictiveScheduling.ValueObjects;
using PlantSafety.Domain.Scheduling.PredictiveScheduling.ValueObjects.Ids;

namespace PlantSafety.Application.Scheduling.PredictiveScheduling.Optimization;

/// <summary>
/// Greedy strategy: places the highest-priority task first, into the earliest resource window
/// that satisfies every constraint rule, and repeats until every task is placed or exhausted.
/// </summary>
public sealed class GreedyOptimizationStrategy : IOptimizationStrategy
{
    private readonly PriorityWeightingService _priorityWeightingService;
    private readonly ResourceAvailabilityModel _availabilityModel;
    private readonly ConstraintEvaluator _constraintEvaluator;

    /// <summary>Creates the strategy with the platform-default rule engine components.</summary>
    public GreedyOptimizationStrategy()
        : this(new PriorityWeightingService(), new ResourceAvailabilityModel(), new ConstraintEvaluator()) { }

    /// <summary>Creates the strategy with explicit rule engine collaborators (for testing/composition).</summary>
    public GreedyOptimizationStrategy(
        PriorityWeightingService priorityWeightingService,
        ResourceAvailabilityModel availabilityModel,
        ConstraintEvaluator constraintEvaluator)
    {
        _priorityWeightingService = priorityWeightingService;
        _availabilityModel = availabilityModel;
        _constraintEvaluator = constraintEvaluator;
    }

    /// <inheritdoc/>
    public string StrategyName => "Greedy";

    /// <inheritdoc/>
    public OptimizationResult Optimize(OptimizationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var orderedTasks = _priorityWeightingService.OrderByPriority(context.Tasks, context.AsOfUtc);
        var committed = new List<ScheduleAssignment>(context.ExistingAssignments);
        var unscheduled = new List<SchedulableTaskId>();
        var tolerated = new List<ConstraintViolation>();

        foreach (var task in orderedTasks)
        {
            var placed = TryPlaceTask(task, context, committed, tolerated);
            if (!placed)
                unscheduled.Add(task.TaskId);
        }

        var newAssignments = committed.Except(context.ExistingAssignments).ToList();

        return new OptimizationResult
        {
            StrategyName = StrategyName,
            Assignments = newAssignments,
            UnscheduledTaskIds = unscheduled,
            ToleratedViolations = tolerated,
            Score = ScoreResult(orderedTasks.Count, newAssignments.Count, tolerated.Count),
        };
    }

    private bool TryPlaceTask(
        SchedulableTask task,
        OptimizationContext context,
        List<ScheduleAssignment> committed,
        List<ConstraintViolation> tolerated)
    {
        var resourceId = task.RequiredMachineId ?? task.RequiredCrewId;
        if (resourceId is null)
            return false;

        foreach (var candidateStart in _availabilityModel.EnumerateCandidateStartTimes(
                     resourceId.Value, task.EstimatedDuration, task.EarliestStartUtc, task.LatestFinishUtc,
                     context.Availability, committed))
        {
            var candidateEnd = UtcTimestamp.From(candidateStart.Value + task.EstimatedDuration);
            var candidate = new ScheduleAssignment(task.TaskId, resourceId.Value, candidateStart, candidateEnd)
            {
                EffectivePriorityWeight = task.BasePriorityWeight,
            };

            var violations = _constraintEvaluator.Evaluate(task, candidate, context.Availability, committed);
            if (violations.Any(v => v.Severity == Domain.Scheduling.PredictiveScheduling.Enums.ConstraintSeverity.Blocking))
                continue;

            committed.Add(candidate);
            tolerated.AddRange(violations);
            return true;
        }

        return false;
    }

    private static double ScoreResult(int totalTasks, int scheduledCount, int toleratedViolationCount)
    {
        if (totalTasks == 0)
            return 1.0;

        var coverage = scheduledCount / (double)totalTasks;
        var violationPenalty = toleratedViolationCount * 0.02;
        return Math.Round(Math.Max(0.0, coverage - violationPenalty), 4);
    }
}

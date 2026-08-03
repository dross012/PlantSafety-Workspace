// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: Event-driven scheduling orchestration (Phase 5, Section 1.4). Reacts to
// cross-module domain signals -- Operational.PredictiveMaintenance/PreventiveMaintenance
// escalations, Operational production/state changes, RiskManagement escalations, and
// ContinuousImprovement (CAPA/RCA) updates -- by recomputing task priority weights via
// PriorityWeightingService so a subsequent MultiObjectiveScheduleOptimizer run reflects the
// new urgency. This orchestrator is intentionally infrastructure-agnostic: it is invoked by
// MediatR command/notification handlers (Application layer) once those cross-module domain
// events are dispatched by the Infrastructure-layer IDomainEventDispatcher; it performs no
// I/O itself. SchemaVersion: 1

using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Scheduling.PredictiveScheduling.Services;
using PlantSafety.Domain.Scheduling.PredictiveScheduling.ValueObjects;

namespace PlantSafety.Application.Scheduling.PredictiveScheduling.Orchestration;

/// <summary>Categorizes the cross-module signal that triggered a reprioritization pass.</summary>
public enum ReprioritizationTrigger
{
    /// <summary>A PredictiveMaintenance/PreventiveMaintenance event indicated the task is now more urgent (e.g. failure likelihood crossed the alert threshold).</summary>
    MaintenanceEscalation = 0,

    /// <summary>A production/operational state change (e.g. unplanned downtime) requires the schedule to adjust.</summary>
    ProductionStateChange = 1,

    /// <summary>A RiskManagement escalation raised the risk associated with a pending task.</summary>
    RiskEscalation = 2,

    /// <summary>A ContinuousImprovement (CAPA/RCA) update changed the corrective-action urgency for a task.</summary>
    ContinuousImprovementUpdate = 3,
}

/// <summary>The outcome of reprioritizing a single task in response to a cross-module trigger.</summary>
public sealed record ReprioritizationOutcome
{
    /// <summary>The task as it stood before reprioritization.</summary>
    public required SchedulableTask OriginalTask { get; init; }

    /// <summary>The task with its urgency flags/priority weight updated to reflect the trigger.</summary>
    public required SchedulableTask UpdatedTask { get; init; }

    /// <summary>Trigger category that caused this reprioritization.</summary>
    public required ReprioritizationTrigger Trigger { get; init; }

    /// <summary>Composite priority weight before the update.</summary>
    public required double PreviousWeight { get; init; }

    /// <summary>Composite priority weight after the update.</summary>
    public required double NewWeight { get; init; }

    /// <summary>True when the recomputed weight increased enough that the optimizer should be re-run promptly.</summary>
    public bool RequiresImmediateReoptimization => NewWeight - PreviousWeight >= 20.0;
}

/// <summary>
/// Recomputes predictive scheduling task priority weights in response to cross-module domain
/// signals, so the optimization loop (Section 1.3) reflects newly-surfaced urgency without
/// requiring a full aggregate reload.
/// </summary>
public sealed class SchedulingReprioritizationOrchestrator
{
    private readonly PriorityWeightingService _priorityWeightingService;

    /// <summary>Creates the orchestrator over the given priority weighting service.</summary>
    public SchedulingReprioritizationOrchestrator(PriorityWeightingService priorityWeightingService)
    {
        _priorityWeightingService = priorityWeightingService;
    }

    /// <summary>
    /// Reprioritizes <paramref name="task"/> in response to a PredictiveMaintenance/PreventiveMaintenance
    /// escalation (e.g. a FailureLikelihoodPredictor alert): marks the task overdue so the priority
    /// weighting's overdue bonus applies.
    /// </summary>
    public ReprioritizationOutcome ReprioritizeForMaintenanceEscalation(SchedulableTask task, UtcTimestamp asOfUtc)
    {
        ArgumentNullException.ThrowIfNull(task);
        var previousWeight = _priorityWeightingService.ComputeWeight(task, asOfUtc);

        var updated = task with { IsOverdue = true };
        var newWeight = _priorityWeightingService.ComputeWeight(updated, asOfUtc);

        return BuildOutcome(task, updated, ReprioritizationTrigger.MaintenanceEscalation, previousWeight, newWeight);
    }

    /// <summary>
    /// Reprioritizes <paramref name="task"/> in response to a production/operational state change
    /// (e.g. unplanned downtime): increments the deferral count to reflect the schedule slip.
    /// </summary>
    public ReprioritizationOutcome ReprioritizeForProductionStateChange(SchedulableTask task, UtcTimestamp asOfUtc)
    {
        ArgumentNullException.ThrowIfNull(task);
        var previousWeight = _priorityWeightingService.ComputeWeight(task, asOfUtc);

        var updated = task with { DeferralCount = task.DeferralCount + 1 };
        var newWeight = _priorityWeightingService.ComputeWeight(updated, asOfUtc);

        return BuildOutcome(task, updated, ReprioritizationTrigger.ProductionStateChange, previousWeight, newWeight);
    }

    /// <summary>
    /// Reprioritizes <paramref name="task"/> in response to a RiskManagement escalation: marks the task
    /// safety-critical so the safety-critical bonus and SafetyRuleSet constraints apply going forward.
    /// </summary>
    public ReprioritizationOutcome ReprioritizeForRiskEscalation(SchedulableTask task, UtcTimestamp asOfUtc)
    {
        ArgumentNullException.ThrowIfNull(task);
        var previousWeight = _priorityWeightingService.ComputeWeight(task, asOfUtc);

        var updated = task with { IsSafetyCritical = true };
        var newWeight = _priorityWeightingService.ComputeWeight(updated, asOfUtc);

        return BuildOutcome(task, updated, ReprioritizationTrigger.RiskEscalation, previousWeight, newWeight);
    }

    /// <summary>
    /// Reprioritizes <paramref name="task"/> in response to a ContinuousImprovement (CAPA/RCA) update
    /// that identified this task as a corrective action: increments the deferral count so it competes
    /// more strongly for scarce resource windows.
    /// </summary>
    public ReprioritizationOutcome ReprioritizeForContinuousImprovementUpdate(SchedulableTask task, UtcTimestamp asOfUtc)
    {
        ArgumentNullException.ThrowIfNull(task);
        var previousWeight = _priorityWeightingService.ComputeWeight(task, asOfUtc);

        var updated = task with { DeferralCount = task.DeferralCount + 1 };
        var newWeight = _priorityWeightingService.ComputeWeight(updated, asOfUtc);

        return BuildOutcome(task, updated, ReprioritizationTrigger.ContinuousImprovementUpdate, previousWeight, newWeight);
    }

    private static ReprioritizationOutcome BuildOutcome(
        SchedulableTask original, SchedulableTask updated, ReprioritizationTrigger trigger,
        double previousWeight, double newWeight) => new()
    {
        OriginalTask = original,
        UpdatedTask = updated,
        Trigger = trigger,
        PreviousWeight = previousWeight,
        NewWeight = newWeight,
    };
}

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PredictiveScheduling
//  Layer:          Domain
//  File:           PriorityWeightingService.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Phase 5 Predictive Scheduling Engine)
//  SchemaVersion:  2026.1
//  Description:    Domain service computing the composite, deterministic priority weight used
//                  by optimization strategies to order candidate tasks. Combines base priority,
//                  overdue status, safety criticality, deferral history, and remaining slack.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Scheduling.PredictiveScheduling.Interfaces;

namespace PlantSafety.Domain.Scheduling.PredictiveScheduling.Services;

/// <summary>Computes a deterministic composite priority weight for a schedulable task.</summary>
public sealed class PriorityWeightingService : IDomainService
{
    private const double OverdueBonus = 25.0;
    private const double SafetyCriticalBonus = 40.0;
    private const double DeferralBonusPerOccurrence = 10.0;
    private const double MaxDeferralBonus = 40.0;
    private const double LowSlackBonus = 15.0;
    private static readonly TimeSpan LowSlackThreshold = TimeSpan.FromHours(4);

    /// <summary>
    /// Computes the composite priority weight for <paramref name="task"/> as of
    /// <paramref name="asOfUtc"/>. Higher values indicate the task should be scheduled sooner.
    /// </summary>
    public double ComputeWeight(SchedulableTask task, UtcTimestamp asOfUtc) => ComputeWeight(task, asOfUtc, TenantSchedulingRuleOverrides.Default);

    /// <summary>
    /// Computes the composite priority weight for <paramref name="task"/> as of
    /// <paramref name="asOfUtc"/>, applying <paramref name="overrides"/>' safety-critical/overdue
    /// bonus multipliers (Phase 7 §1.4 Tenant-Aware Scheduling -- tenant-specific rule
    /// customization via <see cref="ITenantSchedulingRuleOverrideProvider"/>). Passing
    /// <see cref="TenantSchedulingRuleOverrides.Default"/> reproduces
    /// <see cref="ComputeWeight(SchedulableTask, UtcTimestamp)"/>'s exact original behavior.
    /// </summary>
    public double ComputeWeight(SchedulableTask task, UtcTimestamp asOfUtc, TenantSchedulingRuleOverrides overrides)
    {
        ArgumentNullException.ThrowIfNull(overrides);

        var weight = task.BasePriorityWeight;

        if (task.IsOverdue)
            weight += OverdueBonus * overrides.OverduePriorityMultiplier;

        if (task.IsSafetyCritical)
            weight += SafetyCriticalBonus * overrides.SafetyCriticalPriorityMultiplier;

        weight += Math.Min(task.DeferralCount * DeferralBonusPerOccurrence, MaxDeferralBonus);

        if (task.Slack <= LowSlackThreshold && task.Slack >= TimeSpan.Zero)
            weight += LowSlackBonus;

        // Tasks with negative slack (i.e. it is already mathematically impossible to complete
        // them within their window) are surfaced with the highest possible urgency so the
        // optimizer either schedules them first or flags them for escalation.
        if (task.Slack < TimeSpan.Zero)
            weight += LowSlackBonus * 2;

        return Math.Round(weight, 4);
    }

    /// <summary>
    /// Orders the supplied tasks by descending composite priority weight (ties broken by the
    /// earliest EarliestStartUtc, then by TaskId for full determinism).
    /// </summary>
    public IReadOnlyList<SchedulableTask> OrderByPriority(IEnumerable<SchedulableTask> tasks, UtcTimestamp asOfUtc)
        => OrderByPriority(tasks, asOfUtc, TenantSchedulingRuleOverrides.Default);

    /// <summary>
    /// Orders the supplied tasks by descending composite priority weight computed with
    /// <paramref name="overrides"/> applied (Phase 7 §1.4 Tenant-Aware Scheduling).
    /// </summary>
    public IReadOnlyList<SchedulableTask> OrderByPriority(IEnumerable<SchedulableTask> tasks, UtcTimestamp asOfUtc, TenantSchedulingRuleOverrides overrides)
    {
        ArgumentNullException.ThrowIfNull(tasks);

        return tasks
            .Select(t => (Task: t, Weight: ComputeWeight(t, asOfUtc, overrides)))
            .OrderByDescending(x => x.Weight)
            .ThenBy(x => x.Task.EarliestStartUtc.Value)
            .ThenBy(x => x.Task.TaskId.Value)
            .Select(x => x.Task)
            .ToList();
    }
}

// ============================================================================
//  Project:        Shared.Scheduling.Abstractions
//  Module:         Schedule
//  File:           SchedulingScheduleCalculator.cs
//  Created:        2026-08-10
//  Description:    Phase 14 (Scheduling Modernization) deliverable. Composes
//                  SchedulingRuleEngine.Evaluate and SchedulingDueTimeEngine.
//                  CalculateDueTimes into a single deterministic
//                  SchedulingSchedule -- the one pure entry point every
//                  higher layer (SchedulingEvaluationPipeline, Telemetry,
//                  Authorization, Tenant, Orchestrator) ultimately calls.
//                  Pure function: no I/O, no IClock read (asOfUtc is
//                  supplied by the caller), no randomness.
// ============================================================================
using Shared.Scheduling.Abstractions.DueTime;
using Shared.Scheduling.Abstractions.Rules;

namespace Shared.Scheduling.Abstractions.Schedule;

/// <summary>The deterministic, composed output of a full scheduling evaluation.</summary>
public sealed record SchedulingSchedule(
    IReadOnlyList<ScheduledDueTime> DueTimes,
    SchedulingRuleDecision RuleDecision,
    DateTime EarliestDueAtUtc);

/// <summary>Deterministically composes SchedulingRuleEngine and SchedulingDueTimeEngine into a single SchedulingSchedule.</summary>
public static class SchedulingScheduleCalculator
{
    /// <summary>
    /// Calculates a SchedulingSchedule for <paramref name="context"/> as of <paramref name="asOfUtc"/>.
    /// EarliestDueAtUtc is the worst-case-wins (soonest) DueAtUtc across every ScheduledDueTime, or
    /// DateTime.MaxValue when RequiredActions is empty.
    /// </summary>
    public static SchedulingSchedule Calculate(SchedulingEvaluationContext context, DateTime asOfUtc)
    {
        ArgumentNullException.ThrowIfNull(context);

        IReadOnlyList<ScheduledDueTime> dueTimes = SchedulingDueTimeEngine.CalculateDueTimes(context.RequiredActions, asOfUtc);
        SchedulingRuleDecision ruleDecision = SchedulingRuleEngine.Evaluate(context);

        DateTime earliestDueAtUtc = DateTime.MaxValue;
        foreach (ScheduledDueTime dueTime in dueTimes)
        {
            if (dueTime.DueAtUtc < earliestDueAtUtc)
            {
                earliestDueAtUtc = dueTime.DueAtUtc;
            }
        }

        return new SchedulingSchedule(dueTimes, ruleDecision, earliestDueAtUtc);
    }
}

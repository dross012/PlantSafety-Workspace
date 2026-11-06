// ============================================================================
//  Project:        Shared.Scheduling.Infrastructure
//  Module:         Telemetry
//  File:           SchedulingTelemetry.cs
//  Created:        2026-08-10
//  Description:    Phase 14 (Scheduling Modernization) deliverable. Wraps
//                  Shared.Scheduling.Abstractions.Schedule.
//                  SchedulingScheduleCalculator.Calculate (this phase,
//                  unchanged -- still a pure function) with deterministic
//                  logging, metrics, and reliability reporting, via
//                  Shared.Telemetry's existing ReliabilityHooks/
//                  ILogEventWriter/PlantSafetyMetrics. Does not alter
//                  Calculate's decision logic in any way -- this is a
//                  decorator, not a modification. Exact mirror of
//                  WorkflowEngineTelemetry (Phase 8) and RoutingTelemetry
//                  (Phase 13).
// ============================================================================
using Shared.Deterministic.Abstractions.Clock;
using Shared.Scheduling.Abstractions.DueTime;
using Shared.Scheduling.Abstractions.Rules;
using Shared.Scheduling.Abstractions.Schedule;
using Shared.Scheduling.Infrastructure.Governance;
using Shared.Telemetry.Abstractions.Events;
using Shared.Telemetry.Abstractions.Reliability;

namespace Shared.Scheduling.Infrastructure.Telemetry;

/// <summary>Instruments SchedulingScheduleCalculator.Calculate with deterministic logging, metrics, and reliability reporting.</summary>
public static class SchedulingTelemetry
{
    private const string Module = "Scheduling";

    /// <summary>
    /// Calculates a schedule for <paramref name="context"/> as of <paramref name="asOfUtc"/> via the real
    /// SchedulingScheduleCalculator.Calculate, recording a SchedulingSchedulesComputed metric (tagged by
    /// RequiresImmediateScheduling) and a structured SCHEDULING_SCHEDULE_COMPUTED log event. Never alters
    /// the calculation's result.
    /// </summary>
    public static async Task<SchedulingSchedule> CalculateWithTelemetryAsync(
        SchedulingEvaluationContext context,
        DateTime asOfUtc,
        IClock clock,
        ILogEventWriter logEventWriter,
        string correlationId)
    {
        ArgumentNullException.ThrowIfNull(context);

        SchedulingSchedule schedule = await ReliabilityHooks.ExecuteAsync(
            Module,
            nameof(SchedulingScheduleCalculator.Calculate),
            clock,
            logEventWriter,
            correlationId,
            () => Task.FromResult(SchedulingScheduleCalculator.Calculate(context, asOfUtc)));

        SchedulingDueTimeStatus status = SchedulingDueTimeEvaluator.EvaluateStatus(schedule.DueTimes, asOfUtc);

        SchedulingMetrics.SchedulesComputed.Add(1, new KeyValuePair<string, object?>("requiresImmediateScheduling", schedule.RuleDecision.RequiresImmediateScheduling));
        SchedulingMetrics.DueTimeEvaluationsPerformed.Add(1, new KeyValuePair<string, object?>("status", status.ToString()));

        if (schedule.RuleDecision.RequiresImmediateScheduling)
        {
            SchedulingMetrics.ImmediateSchedulingRequiredCount.Add(1);
        }

        logEventWriter.Write(new LogEvent(
            EventId: SchedulingGovernanceEvents.ScheduleComputed,
            Category: LogCategory.Scheduling,
            Severity: LogSeverity.Info,
            Message: $"SchedulingScheduleCalculator.Calculate produced {schedule.RuleDecision.TotalRequiredActionCount} due time(s), status {status}, earliest due {schedule.EarliestDueAtUtc:O}.",
            Payload: new Dictionary<string, object?>
            {
                ["TotalRequiredActionCount"] = schedule.RuleDecision.TotalRequiredActionCount,
                ["TotalEstimatedEffortHours"] = schedule.RuleDecision.TotalEstimatedEffortHours,
                ["WorstCaseDueWithinHours"] = schedule.RuleDecision.WorstCaseDueWithinHours,
                ["RequiresImmediateScheduling"] = schedule.RuleDecision.RequiresImmediateScheduling,
                ["Status"] = status.ToString(),
                ["EarliestDueAtUtc"] = schedule.EarliestDueAtUtc,
            },
            CorrelationId: correlationId,
            Module: Module,
            OccurredAtUtc: clock.UtcNow));

        return schedule;
    }
}

// ============================================================================
//  Project:        Shared.Scheduling.Infrastructure
//  Module:         Orchestration
//  File:           SchedulingEventEmitter.cs
//  Created:        2026-08-10
//  Description:    Phase 14 (Scheduling Modernization) deliverable. Emits a
//                  SchedulingScheduledEvent (Shared.Scheduling.Abstractions.
//                  Events, this phase) as a structured LogEvent -- the
//                  unified event schema (Phase 12 precedent: no new event
//                  bus/envelope type, the existing LogEvent.Payload IS the
//                  unified event schema) -- via the existing
//                  ILogEventWriter seam (Phase 2.4). Pure emission: builds
//                  the event from an already-computed SchedulingEvaluationResultContract,
//                  never recomputes the schedule.
// ============================================================================
using Shared.Scheduling.Abstractions.Contracts;
using Shared.Scheduling.Abstractions.Events;
using Shared.Scheduling.Infrastructure.Governance;
using Shared.Telemetry.Abstractions.Events;

namespace Shared.Scheduling.Infrastructure.Orchestration;

/// <summary>Emits a SchedulingScheduledEvent as a structured LogEvent for an already-computed scheduling result.</summary>
public static class SchedulingEventEmitter
{
    private const string Module = "Scheduling";

    /// <summary>
    /// Builds a SchedulingScheduledEvent from <paramref name="result"/> and writes one
    /// SCHEDULING_SCHEDULED_EVENT_PUBLISHED log event carrying it.
    /// </summary>
    public static SchedulingScheduledEvent Emit(SchedulingEvaluationResultContract result, ILogEventWriter logEventWriter)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(logEventWriter);

        var scheduledEvent = new SchedulingScheduledEvent(
            CorrelationId: result.Metadata.CorrelationId,
            TenantId: result.Metadata.TenantId,
            AsOfUtc: result.Metadata.IssuedAtUtc,
            EarliestDueAtUtc: result.Schedule.EarliestDueAtUtc,
            Status: result.Status,
            TotalRequiredActionCount: result.Schedule.RuleDecision.TotalRequiredActionCount,
            TotalEstimatedEffortHours: result.Schedule.RuleDecision.TotalEstimatedEffortHours,
            RequiresImmediateScheduling: result.Schedule.RuleDecision.RequiresImmediateScheduling);

        logEventWriter.Write(new LogEvent(
            EventId: SchedulingGovernanceEvents.ScheduledEventPublished,
            Category: LogCategory.Scheduling,
            Severity: LogSeverity.Info,
            Message: $"Published SchedulingScheduledEvent for tenant \"{scheduledEvent.TenantId}\": status {scheduledEvent.Status}, earliest due {scheduledEvent.EarliestDueAtUtc:O}.",
            Payload: new Dictionary<string, object?>
            {
                ["TenantId"] = scheduledEvent.TenantId,
                ["AsOfUtc"] = scheduledEvent.AsOfUtc,
                ["EarliestDueAtUtc"] = scheduledEvent.EarliestDueAtUtc,
                ["Status"] = scheduledEvent.Status.ToString(),
                ["TotalRequiredActionCount"] = scheduledEvent.TotalRequiredActionCount,
                ["TotalEstimatedEffortHours"] = scheduledEvent.TotalEstimatedEffortHours,
                ["RequiresImmediateScheduling"] = scheduledEvent.RequiresImmediateScheduling,
            },
            CorrelationId: scheduledEvent.CorrelationId,
            Module: Module,
            OccurredAtUtc: scheduledEvent.AsOfUtc));

        return scheduledEvent;
    }
}

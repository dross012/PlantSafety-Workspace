// ============================================================================
//  Project:        Shared.Scheduling.Infrastructure
//  Module:         Governance
//  File:           SchedulingGovernanceEvents.cs
//  Created:        2026-08-10
//  Description:    Phase 14 (Scheduling Modernization) deliverable. The
//                  fixed set of EventId literals Scheduling's telemetry and
//                  governance layers write -- centralized here so every
//                  emitter (SchedulingTelemetry, SchedulingEventEmitter,
//                  SchedulingGovernanceEvaluator) uses the exact same
//                  UPPER_SNAKE_CASE string, satisfying
//                  TelemetryGovernanceRules.ValidateLogEvent's EventId
//                  pattern by construction rather than by convention alone.
// ============================================================================
namespace Shared.Scheduling.Infrastructure.Governance;

/// <summary>The fixed set of EventId literals Scheduling's telemetry and governance layers write.</summary>
public static class SchedulingGovernanceEvents
{
    /// <summary>Emitted by SchedulingTelemetry after a schedule is computed.</summary>
    public const string ScheduleComputed = "SCHEDULING_SCHEDULE_COMPUTED";

    /// <summary>Emitted by SchedulingEventEmitter after a SchedulingScheduledEvent is published.</summary>
    public const string ScheduledEventPublished = "SCHEDULING_SCHEDULED_EVENT_PUBLISHED";

    /// <summary>Emitted by SchedulingGovernanceEvaluator when a governance rule violation is found.</summary>
    public const string GovernanceViolation = "SCHEDULING_GOVERNANCE_VIOLATION";

    /// <summary>Emitted by SchedulingOrchestrator once a full evaluation completes.</summary>
    public const string OrchestrationCompleted = "SCHEDULING_ORCHESTRATION_COMPLETED";

    /// <summary>Emitted by SchedulingTracing for each recorded orchestration span.</summary>
    public const string TraceSpan = "SCHEDULING_TRACE_SPAN";
}

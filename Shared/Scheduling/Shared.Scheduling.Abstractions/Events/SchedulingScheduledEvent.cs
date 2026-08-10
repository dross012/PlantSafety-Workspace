// ============================================================================
//  Project:        Shared.Scheduling.Abstractions
//  Module:         Events
//  File:           SchedulingScheduledEvent.cs
//  Created:        2026-08-10
//  Description:    Phase 14 (Scheduling Modernization) deliverable. The
//                  unified-schema (Phase 12) event payload emitted once a
//                  SchedulingSchedule has been computed -- carried inside an
//                  ordinary Shared.Telemetry LogEvent.Payload (Phase 2.4),
//                  not a new event bus or envelope type, exactly as Phase
//                  12's own precedent established (no UnifiedEventBase was
//                  invented; the existing LogEvent shape is the unified
//                  event schema).
// ============================================================================
using Shared.Scheduling.Abstractions.DueTime;

namespace Shared.Scheduling.Abstractions.Events;

/// <summary>The deterministic, unified-schema payload describing a completed scheduling evaluation.</summary>
public sealed record SchedulingScheduledEvent(
    string CorrelationId,
    string TenantId,
    DateTime AsOfUtc,
    DateTime EarliestDueAtUtc,
    SchedulingDueTimeStatus Status,
    int TotalRequiredActionCount,
    int TotalEstimatedEffortHours,
    bool RequiresImmediateScheduling);

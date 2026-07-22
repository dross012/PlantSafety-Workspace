// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PMScheduling
//  Layer:          Domain
//  File:           PMSchedulingScheduledEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Raised when a preventive maintenance task is placed on the PM calendar with a
//                  due date ("PM calendar", "Automated PM reminders").
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.PMScheduling.Events;

/// <summary>Raised when a <see cref="PMScheduling"/> task is scheduled with a due date.</summary>
public sealed record PMSchedulingScheduledEvent(
    PMSchedulingEventId EventId,
    PMSchedulingId      PMSchedulingId,
    UtcTimestamp        DueAtUtc,
    UtcTimestamp        ScheduledAtUtc,
    PerformedByActor    Actor,
    CorrelationId       CorrelationId,
    CausationId         CausationId
) : IPMSchedulingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PMSchedulingScheduled";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ScheduledAtUtc;
}

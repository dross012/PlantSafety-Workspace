// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PMScheduling
//  Layer:          Domain
//  File:           PMSchedulingCancelledEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Raised when a scheduled preventive maintenance task is cancelled (e.g., the
//                  equipment was decommissioned before the PM task came due).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.PMScheduling.Events;

/// <summary>Raised when a <see cref="PMScheduling"/> task is cancelled.</summary>
public sealed record PMSchedulingCancelledEvent(
    PMSchedulingEventId EventId,
    PMSchedulingId      PMSchedulingId,
    string              Reason,
    UtcTimestamp        CancelledAtUtc,
    PerformedByActor    Actor,
    CorrelationId       CorrelationId,
    CausationId         CausationId
) : IPMSchedulingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PMSchedulingCancelled";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CancelledAtUtc;
}

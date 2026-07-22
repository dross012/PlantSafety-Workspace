// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PMScheduling
//  Layer:          Domain
//  File:           PMSchedulingCompletedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Raised when a preventive maintenance task is completed ("PM compliance
//                  reporting", "Safety device inspection logs").
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.PMScheduling.Events;

/// <summary>Raised when a <see cref="PMScheduling"/> task is completed.</summary>
public sealed record PMSchedulingCompletedEvent(
    PMSchedulingEventId EventId,
    PMSchedulingId      PMSchedulingId,
    string?             CompletionNotes,
    UtcTimestamp        CompletedAtUtc,
    PerformedByActor    Actor,
    CorrelationId       CorrelationId,
    CausationId         CausationId
) : IPMSchedulingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PMSchedulingCompleted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CompletedAtUtc;
}

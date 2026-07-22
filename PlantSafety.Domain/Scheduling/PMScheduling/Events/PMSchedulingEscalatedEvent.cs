// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PMScheduling
//  Layer:          Domain
//  File:           PMSchedulingEscalatedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Raised when a preventive maintenance task passes its due date unresolved and
//                  is marked overdue ("Tracking overdue PM tasks"), carrying the assigned
//                  Work Order Priority code (P1-P4).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.PMScheduling.Events;

/// <summary>Raised when a <see cref="PMScheduling"/> task becomes overdue and is escalated.</summary>
public sealed record PMSchedulingEscalatedEvent(
    PMSchedulingEventId EventId,
    PMSchedulingId      PMSchedulingId,
    string              PriorityCode,
    UtcTimestamp        EscalatedAtUtc,
    PerformedByActor    Actor,
    CorrelationId       CorrelationId,
    CausationId         CausationId
) : IPMSchedulingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PMSchedulingEscalated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => EscalatedAtUtc;
}

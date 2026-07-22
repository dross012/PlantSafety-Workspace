// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.HazardIsolationSequencing
//  Layer:          Domain
//  File:           HazardIsolationSequencingCreatedEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Raised when a new HazardIsolationSequencing aggregate is created.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.HazardIsolationSequencing.Events;

/// <summary>Raised when a new <see cref="HazardIsolationSequencing"/> aggregate is created.</summary>
public sealed record HazardIsolationSequencingCreatedEvent(
    HazardIsolationSequencingEventId EventId,
    HazardIsolationSequencingId      HazardIsolationSequencingId,
    UtcTimestamp  CreatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId   CausationId
) : IHazardIsolationSequencingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "HazardIsolationSequencingCreated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CreatedAtUtc;
}
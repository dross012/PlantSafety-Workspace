// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyCulture.StopWorkEvent
//  Layer:          Domain
//  File:           StopWorkEventCreatedEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Raised when a new StopWorkEvent aggregate is created.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyCulture.StopWorkEvent.Events;

/// <summary>Raised when a new <see cref="StopWorkEvent"/> aggregate is created.</summary>
public sealed record StopWorkEventCreatedEvent(
    StopWorkEventEventId EventId,
    StopWorkEventId      StopWorkEventId,
    UtcTimestamp  CreatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId   CausationId
) : IStopWorkEventDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "StopWorkEventCreated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CreatedAtUtc;
}
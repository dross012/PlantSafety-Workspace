// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.WasteStreams
//  Layer:          Domain
//  File:           WasteStreamEvents.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain events raised across the WasteStream lifecycle: characterization,
//                  re-characterization (versioned, never overwritten), and deactivation.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.WasteStreams.Enums;
using PlantSafety.Domain.HazardousWasteManagement.WasteStreams.ValueObjects.Ids;

namespace PlantSafety.Domain.HazardousWasteManagement.WasteStreams.Events;

/// <summary>Raised when a waste stream is characterized for the first time.</summary>
public sealed record WasteStreamCharacterizedEvent(
    WasteStreamEventId EventId,
    WasteStreamId WasteStreamId,
    string EpaWasteCode,
    WasteHazardClass HazardClass,
    int CharacterizationVersion,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IWasteStreamDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "WasteStreamCharacterized";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when an existing waste stream is re-characterized. Prior characterization
/// data is retained as history; this event bumps <see cref="CharacterizationVersion"/> rather
/// than overwriting it.</summary>
public sealed record WasteStreamRecharacterizedEvent(
    WasteStreamEventId EventId,
    WasteStreamId WasteStreamId,
    string EpaWasteCode,
    WasteHazardClass HazardClass,
    int CharacterizationVersion,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IWasteStreamDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "WasteStreamRecharacterized";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when a waste stream is deactivated.</summary>
public sealed record WasteStreamDeactivatedEvent(
    WasteStreamEventId EventId,
    WasteStreamId WasteStreamId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IWasteStreamDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "WasteStreamDeactivated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

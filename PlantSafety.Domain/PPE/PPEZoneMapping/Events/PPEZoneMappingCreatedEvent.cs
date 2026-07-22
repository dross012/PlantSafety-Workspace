// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPEZoneMapping
//  Layer:          Domain
//  File:           PPEZoneMappingCreatedEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Raised when a new PPEZoneMapping aggregate is created.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPEZoneMapping.Events;

/// <summary>Raised when a new <see cref="PPEZoneMapping"/> aggregate is created.</summary>
public sealed record PPEZoneMappingCreatedEvent(
    PPEZoneMappingEventId EventId,
    PPEZoneMappingId      PPEZoneMappingId,
    UtcTimestamp  CreatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId   CausationId
) : IPPEZoneMappingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PPEZoneMappingCreated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CreatedAtUtc;
}
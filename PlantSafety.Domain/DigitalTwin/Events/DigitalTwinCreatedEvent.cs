// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin
//  Layer:          Domain
//  File:           DigitalTwinCreatedEvent.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain event raised when a DigitalTwin aggregate is created.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.Events;

/// <summary>Raised when a new <see cref="DigitalTwin"/> aggregate is created.</summary>
public sealed record DigitalTwinCreatedEvent(
    Guid          EventId,
    Guid          DigitalTwinId,
    UtcTimestamp  CreatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId   CausationId
) : IDigitalTwinDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "DigitalTwinCreated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CreatedAtUtc;
}

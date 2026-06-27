// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability
//  Layer:          Domain
//  File:           SustainabilityCreatedEvent.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain event raised when a Sustainability aggregate is created.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Sustainability.Events;

/// <summary>Raised when a new <see cref="Sustainability"/> aggregate is created.</summary>
public sealed record SustainabilityCreatedEvent(
    Guid          EventId,
    Guid          SustainabilityId,
    UtcTimestamp  CreatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId   CausationId
) : ISustainabilityDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "SustainabilityCreated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CreatedAtUtc;
}

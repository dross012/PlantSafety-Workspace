// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth
//  Layer:          Domain
//  File:           EnvironmentalHealthCreatedEvent.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain event raised when a EnvironmentalHealth aggregate is created.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.Events;

/// <summary>Raised when a new <see cref="EnvironmentalHealth"/> aggregate is created.</summary>
public sealed record EnvironmentalHealthCreatedEvent(
    Guid          EventId,
    Guid          EnvironmentalHealthId,
    UtcTimestamp  CreatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId   CausationId
) : IEnvironmentalHealthDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EnvironmentalHealthCreated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CreatedAtUtc;
}

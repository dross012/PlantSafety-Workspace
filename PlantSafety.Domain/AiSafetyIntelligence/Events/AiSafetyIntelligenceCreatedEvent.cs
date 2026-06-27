// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AiSafetyIntelligence
//  Layer:          Domain
//  File:           AiSafetyIntelligenceCreatedEvent.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain event raised when a AiSafetyIntelligence aggregate is created.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AiSafetyIntelligence.Events;

/// <summary>Raised when a new <see cref="AiSafetyIntelligence"/> aggregate is created.</summary>
public sealed record AiSafetyIntelligenceCreatedEvent(
    Guid          EventId,
    Guid          AiSafetyIntelligenceId,
    UtcTimestamp  CreatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId   CausationId
) : IAiSafetyIntelligenceDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AiSafetyIntelligenceCreated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CreatedAtUtc;
}

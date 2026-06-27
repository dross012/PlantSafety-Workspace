// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AiGovernance
//  Layer:          Domain
//  File:           AiGovernanceCreatedEvent.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain event raised when a AiGovernance aggregate is created.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AiGovernance.Events;

/// <summary>Raised when a new <see cref="AiGovernance"/> aggregate is created.</summary>
public sealed record AiGovernanceCreatedEvent(
    Guid          EventId,
    Guid          AiGovernanceId,
    UtcTimestamp  CreatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId   CausationId
) : IAiGovernanceDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AiGovernanceCreated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CreatedAtUtc;
}

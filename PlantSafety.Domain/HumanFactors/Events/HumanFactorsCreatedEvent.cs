// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors
//  Layer:          Domain
//  File:           HumanFactorsCreatedEvent.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain event raised when a HumanFactors aggregate is created.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HumanFactors.Events;

/// <summary>Raised when a new <see cref="HumanFactors"/> aggregate is created.</summary>
public sealed record HumanFactorsCreatedEvent(
    Guid          EventId,
    Guid          HumanFactorsId,
    UtcTimestamp  CreatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId   CausationId
) : IHumanFactorsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "HumanFactorsCreated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CreatedAtUtc;
}

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnterpriseFabric
//  Layer:          Domain
//  File:           EnterpriseFabricCreatedEvent.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain event raised when a EnterpriseFabric aggregate is created.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnterpriseFabric.Events;

/// <summary>Raised when a new <see cref="EnterpriseFabric"/> aggregate is created.</summary>
public sealed record EnterpriseFabricCreatedEvent(
    Guid          EventId,
    Guid          EnterpriseFabricId,
    UtcTimestamp  CreatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId   CausationId
) : IEnterpriseFabricDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EnterpriseFabricCreated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CreatedAtUtc;
}

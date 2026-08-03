// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.ProductionTracking
//  Layer:          Domain
//  File:           ProductionLineRegisteredEvent.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Raised when a new ProductionLine master-data record is registered.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Operational.ProductionTracking.ValueObjects.Ids;

namespace PlantSafety.Domain.Operational.ProductionTracking.Events;

/// <summary>Raised when a new ProductionLine is registered.</summary>
public sealed record ProductionLineRegisteredEvent(
    Guid EventId,
    ProductionLineId ProductionLineId,
    string LineCode,
    UtcTimestamp CreatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId) : IProductionTrackingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ProductionLineRegistered";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CreatedAtUtc;
}

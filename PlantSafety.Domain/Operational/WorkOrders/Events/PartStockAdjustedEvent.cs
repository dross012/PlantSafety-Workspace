// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.WorkOrders
//  Layer:          Domain
//  File:           PartStockAdjustedEvent.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Raised when a Part's on-hand quantity is adjusted. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Operational.WorkOrders.ValueObjects.Ids;

namespace PlantSafety.Domain.Operational.WorkOrders.Events;

/// <summary>Raised when a Part's on-hand quantity is adjusted.</summary>
public sealed record PartStockAdjustedEvent(
    Guid EventId,
    PartId PartId,
    int DeltaQuantity,
    int NewQuantityOnHand,
    string? Reason,
    UtcTimestamp AdjustedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId) : IPartDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PartStockAdjusted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => AdjustedAtUtc;
}

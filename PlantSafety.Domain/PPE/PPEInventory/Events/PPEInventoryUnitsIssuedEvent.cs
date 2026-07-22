// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPEInventory
//  Layer:          Domain
//  File:           PPEInventoryUnitsIssuedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Raised when units are issued out of a PPEInventory item's on-hand stock,
//                  and when the resulting available balance falls to or below the reorder
//                  threshold ("Automated reorder alerts").
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPEInventory.Events;

/// <summary>Raised when units are issued out of a <see cref="PPEInventory"/> item's on-hand stock.</summary>
public sealed record PPEInventoryUnitsIssuedEvent(
    PPEInventoryEventId EventId,
    PPEInventoryId      PPEInventoryId,
    int                 QuantityIssued,
    int                 RemainingAvailable,
    bool                LowStockTriggered,
    UtcTimestamp        IssuedAtUtc,
    PerformedByActor    Actor,
    CorrelationId       CorrelationId,
    CausationId         CausationId
) : IPPEInventoryDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PPEInventoryUnitsIssued";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => IssuedAtUtc;
}

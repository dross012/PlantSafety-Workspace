// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPEInventory
//  Layer:          Domain
//  File:           PPEInventoryStockReceivedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Raised when new stock is received into a PPEInventory item's on-hand balance.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPEInventory.Events;

/// <summary>Raised when new stock is received into a <see cref="PPEInventory"/> item.</summary>
public sealed record PPEInventoryStockReceivedEvent(
    PPEInventoryEventId EventId,
    PPEInventoryId      PPEInventoryId,
    int                 QuantityReceived,
    int                 NewStockQuantity,
    UtcTimestamp        ReceivedAtUtc,
    PerformedByActor    Actor,
    CorrelationId       CorrelationId,
    CausationId         CausationId
) : IPPEInventoryDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PPEInventoryStockReceived";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ReceivedAtUtc;
}

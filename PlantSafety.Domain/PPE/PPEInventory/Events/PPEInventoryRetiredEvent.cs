// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPEInventory
//  Layer:          Domain
//  File:           PPEInventoryRetiredEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Raised when a PPEInventory item is retired from active service and may no
//                  longer be issued.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPEInventory.Events;

/// <summary>Raised when a <see cref="PPEInventory"/> item is retired from active service.</summary>
public sealed record PPEInventoryRetiredEvent(
    PPEInventoryEventId EventId,
    PPEInventoryId      PPEInventoryId,
    UtcTimestamp        RetiredAtUtc,
    PerformedByActor    Actor,
    CorrelationId       CorrelationId,
    CausationId         CausationId
) : IPPEInventoryDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PPEInventoryRetired";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => RetiredAtUtc;
}

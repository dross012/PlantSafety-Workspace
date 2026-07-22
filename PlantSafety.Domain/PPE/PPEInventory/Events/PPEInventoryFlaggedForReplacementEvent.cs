// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPEInventory
//  Layer:          Domain
//  File:           PPEInventoryFlaggedForReplacementEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Raised when a PPEInventory item is flagged for replacement due to inspection
//                  failure, damage, or approaching expiration ("Replacement thresholds").
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPEInventory.Events;

/// <summary>Raised when a <see cref="PPEInventory"/> item is flagged for replacement.</summary>
public sealed record PPEInventoryFlaggedForReplacementEvent(
    PPEInventoryEventId EventId,
    PPEInventoryId      PPEInventoryId,
    string              Reason,
    UtcTimestamp        FlaggedAtUtc,
    PerformedByActor    Actor,
    CorrelationId       CorrelationId,
    CausationId         CausationId
) : IPPEInventoryDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PPEInventoryFlaggedForReplacement";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => FlaggedAtUtc;
}

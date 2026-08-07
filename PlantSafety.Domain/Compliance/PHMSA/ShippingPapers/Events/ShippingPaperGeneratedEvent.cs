// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.ShippingPapers
//  Layer:          Domain
//  File:           ShippingPaperGeneratedEvent.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Raised when a shipping paper is attached to a HazmatShipment (Draft/Validated
//                  -> Validated).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.ValueObjects.Ids;

namespace PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.Events;

/// <summary>Raised when a shipping paper is attached to a shipment.</summary>
public sealed record ShippingPaperGeneratedEvent(
    HazmatShipmentEventId EventId,
    HazmatShipmentId HazmatShipmentId,
    string BasicDescriptionSequence,
    UtcTimestamp GeneratedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IHazmatShipmentDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ShippingPaperGenerated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => GeneratedAtUtc;
}

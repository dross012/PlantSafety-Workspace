// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.ShippingPapers
//  Layer:          Domain
//  File:           HazmatShipmentReadyEvent.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Raised when a HazmatShipment is marked ready for dispatch (Validated -> Ready).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.ValueObjects.Ids;

namespace PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.Events;

/// <summary>Raised when a shipment is marked ready for dispatch.</summary>
public sealed record HazmatShipmentReadyEvent(
    HazmatShipmentEventId EventId,
    HazmatShipmentId HazmatShipmentId,
    UtcTimestamp ReadyAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IHazmatShipmentDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "HazmatShipmentReady";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ReadyAtUtc;
}

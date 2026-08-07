// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.ShippingPapers
//  Layer:          Domain
//  File:           HazmatShipmentDispatchedEvent.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Raised when a HazmatShipment is dispatched (Ready -> Dispatched).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.ValueObjects.Ids;

namespace PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.Events;

/// <summary>Raised when a shipment is dispatched.</summary>
public sealed record HazmatShipmentDispatchedEvent(
    HazmatShipmentEventId EventId,
    HazmatShipmentId HazmatShipmentId,
    UtcTimestamp DispatchedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IHazmatShipmentDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "HazmatShipmentDispatched";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => DispatchedAtUtc;
}

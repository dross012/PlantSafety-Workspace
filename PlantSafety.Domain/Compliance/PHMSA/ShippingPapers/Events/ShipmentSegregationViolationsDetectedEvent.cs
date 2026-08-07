// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.ShippingPapers
//  Layer:          Domain
//  File:           ShipmentSegregationViolationsDetectedEvent.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Raised when a segregation evaluation recorded against a HazmatShipment finds
//                  one or more violations. Does not itself change the shipment's lifecycle status.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Segregation.ValueObjects;
using PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.ValueObjects.Ids;

namespace PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.Events;

/// <summary>Raised when a recorded segregation evaluation contains one or more violations.</summary>
public sealed record ShipmentSegregationViolationsDetectedEvent(
    HazmatShipmentEventId EventId,
    HazmatShipmentId HazmatShipmentId,
    IReadOnlyList<SegregationViolation> Violations,
    UtcTimestamp DetectedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IHazmatShipmentDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ShipmentSegregationViolationsDetected";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => DetectedAtUtc;
}

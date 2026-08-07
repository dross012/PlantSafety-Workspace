// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.ShippingPapers
//  Layer:          Domain
//  File:           ShipmentNotReadyForDispatchException.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Thrown when MarkReady is called but the shipment lacks an attached shipping
//                  paper or still has unresolved Prohibited-severity segregation violations.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.ValueObjects.Ids;

namespace PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.Exceptions;

/// <summary>
/// Thrown when a <see cref="Aggregates.HazmatShipment"/> cannot be marked ready for dispatch
/// because a shipping paper has not been attached, or because unresolved Prohibited-severity
/// segregation violations remain recorded against it.
/// </summary>
public sealed class ShipmentNotReadyForDispatchException : HazmatShipmentDomainException
{
    /// <summary>Creates an exception describing why the shipment is not ready for dispatch.</summary>
    public ShipmentNotReadyForDispatchException(HazmatShipmentId id, string reason, CorrelationId correlationId, CausationId causationId)
        : base(
            $"HazmatShipment '{id}' is not ready for dispatch: {reason}",
            "HazmatShipment.NotReadyForDispatch",
            "Validation",
            "High",
            correlationId,
            causationId)
    {
    }
}

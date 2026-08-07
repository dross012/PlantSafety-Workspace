// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.ShippingPapers
//  Layer:          Domain
//  File:           HazmatShipmentNotFoundException.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Thrown when a HazmatShipment aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.ValueObjects.Ids;

namespace PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.Exceptions;

/// <summary>Thrown when a <see cref="Aggregates.HazmatShipment"/> aggregate lookup finds no match.</summary>
public sealed class HazmatShipmentNotFoundException : HazmatShipmentDomainException
{
    /// <summary>Creates a not-found exception for the given HazmatShipment identity.</summary>
    public HazmatShipmentNotFoundException(HazmatShipmentId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"HazmatShipment with id '{id}' was not found.",
            "HazmatShipment.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}

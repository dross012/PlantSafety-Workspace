// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.ShippingPapers
//  Layer:          Domain
//  File:           HazmatShipmentInvalidStatusTransitionException.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Thrown when a HazmatShipment lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.ValueObjects.Ids;

namespace PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.Exceptions;

/// <summary>Thrown when a <see cref="Aggregates.HazmatShipment"/> lifecycle transition is not permitted.</summary>
public sealed class HazmatShipmentInvalidStatusTransitionException : HazmatShipmentDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public HazmatShipmentInvalidStatusTransitionException(
        HazmatShipmentId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition HazmatShipment '{id}' from '{from}' to '{to}'.",
            "HazmatShipment.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.Carriers
//  Layer:          Domain
//  File:           CarrierInvalidStatusTransitionException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when a Carrier lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.Carriers.Exceptions;

/// <summary>Thrown when a <see cref="Carrier"/> lifecycle transition is not permitted.</summary>
public sealed class CarrierInvalidStatusTransitionException : CarrierDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public CarrierInvalidStatusTransitionException(
        CarrierId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition 'Carrier' {id} from '{from}' to '{to}'.",
            "Carrier.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}

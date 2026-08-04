// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.TransportTrips
//  Layer:          Domain
//  File:           TransportTripInvalidStatusTransitionException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when a TransportTrip lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.TransportTrips.Exceptions;

/// <summary>Thrown when a <see cref="TransportTrip"/> lifecycle transition is not permitted.</summary>
public sealed class TransportTripInvalidStatusTransitionException : TransportTripDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public TransportTripInvalidStatusTransitionException(
        TransportTripId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition 'TransportTrip' {id} from '{from}' to '{to}'.",
            "TransportTrip.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.TransportTrips
//  Layer:          Domain
//  File:           TransportTripNotFoundException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when a TransportTrip aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.TransportTrips.Exceptions;

/// <summary>Thrown when a <see cref="TransportTrip"/> aggregate lookup finds no match.</summary>
public sealed class TransportTripNotFoundException : TransportTripDomainException
{
    /// <summary>Creates a not-found exception for the given TransportTrip identity.</summary>
    public TransportTripNotFoundException(TransportTripId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"TransportTrip with id '{id}' was not found.",
            "TransportTrip.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.TransportTrips
//  Layer:          Domain
//  File:           ITransportTripFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing TransportTrip aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.TransportTrips.Interfaces;

/// <summary>Factory interface for constructing <see cref="TransportTrip"/> aggregates.</summary>
public interface ITransportTripFactory
{
    /// <summary>Creates a new TransportTrip aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.LogisticsTransportSafety.TransportTrips.Aggregates.TransportTrip Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}

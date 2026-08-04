// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.TransportTrips
//  Layer:          Domain
//  File:           TransportTripFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing TransportTrip aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.TransportTrips.Factories;

/// <summary>Default factory implementation for constructing <see cref="TransportTrip"/> aggregates.</summary>
public sealed class TransportTripFactory : ITransportTripFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.LogisticsTransportSafety.TransportTrips.Aggregates.TransportTrip Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.LogisticsTransportSafety.TransportTrips.Aggregates.TransportTrip.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}

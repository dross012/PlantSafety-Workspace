// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.TransportTrips
//  Layer:          Domain
//  File:           ITransportTripRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the TransportTrip aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.TransportTrips.Interfaces;

/// <summary>Domain repository interface for the <see cref="TransportTrip"/> aggregate root.</summary>
public interface ITransportTripRepository
{
    /// <summary>Retrieves a TransportTrip aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.LogisticsTransportSafety.TransportTrips.Aggregates.TransportTrip?> GetByIdAsync(TransportTripId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all TransportTrip aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.LogisticsTransportSafety.TransportTrips.Aggregates.TransportTrip>> ListAsync(TransportTripSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new TransportTrip aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.LogisticsTransportSafety.TransportTrips.Aggregates.TransportTrip aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing TransportTrip aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.LogisticsTransportSafety.TransportTrips.Aggregates.TransportTrip aggregate, CancellationToken cancellationToken = default);
}

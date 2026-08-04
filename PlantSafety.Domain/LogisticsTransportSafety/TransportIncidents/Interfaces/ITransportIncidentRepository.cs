// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.TransportIncidents
//  Layer:          Domain
//  File:           ITransportIncidentRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the TransportIncident aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.TransportIncidents.Interfaces;

/// <summary>Domain repository interface for the <see cref="TransportIncident"/> aggregate root.</summary>
public interface ITransportIncidentRepository
{
    /// <summary>Retrieves a TransportIncident aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.LogisticsTransportSafety.TransportIncidents.Aggregates.TransportIncident?> GetByIdAsync(TransportIncidentId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all TransportIncident aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.LogisticsTransportSafety.TransportIncidents.Aggregates.TransportIncident>> ListAsync(TransportIncidentSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new TransportIncident aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.LogisticsTransportSafety.TransportIncidents.Aggregates.TransportIncident aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing TransportIncident aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.LogisticsTransportSafety.TransportIncidents.Aggregates.TransportIncident aggregate, CancellationToken cancellationToken = default);
}

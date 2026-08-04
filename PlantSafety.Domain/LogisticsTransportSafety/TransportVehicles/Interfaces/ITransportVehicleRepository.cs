// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.TransportVehicles
//  Layer:          Domain
//  File:           ITransportVehicleRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the TransportVehicle aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.TransportVehicles.Interfaces;

/// <summary>Domain repository interface for the <see cref="TransportVehicle"/> aggregate root.</summary>
public interface ITransportVehicleRepository
{
    /// <summary>Retrieves a TransportVehicle aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.LogisticsTransportSafety.TransportVehicles.Aggregates.TransportVehicle?> GetByIdAsync(TransportVehicleId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all TransportVehicle aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.LogisticsTransportSafety.TransportVehicles.Aggregates.TransportVehicle>> ListAsync(TransportVehicleSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new TransportVehicle aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.LogisticsTransportSafety.TransportVehicles.Aggregates.TransportVehicle aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing TransportVehicle aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.LogisticsTransportSafety.TransportVehicles.Aggregates.TransportVehicle aggregate, CancellationToken cancellationToken = default);
}

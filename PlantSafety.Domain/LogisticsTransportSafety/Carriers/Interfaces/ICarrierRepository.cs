// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.Carriers
//  Layer:          Domain
//  File:           ICarrierRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the Carrier aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.Carriers.Interfaces;

/// <summary>Domain repository interface for the <see cref="Carrier"/> aggregate root.</summary>
public interface ICarrierRepository
{
    /// <summary>Retrieves a Carrier aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.LogisticsTransportSafety.Carriers.Aggregates.Carrier?> GetByIdAsync(CarrierId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all Carrier aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.LogisticsTransportSafety.Carriers.Aggregates.Carrier>> ListAsync(CarrierSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new Carrier aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.LogisticsTransportSafety.Carriers.Aggregates.Carrier aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing Carrier aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.LogisticsTransportSafety.Carriers.Aggregates.Carrier aggregate, CancellationToken cancellationToken = default);
}

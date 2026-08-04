// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.LoadSecurementChecks
//  Layer:          Domain
//  File:           ILoadSecurementCheckRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the LoadSecurementCheck aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.LoadSecurementChecks.Interfaces;

/// <summary>Domain repository interface for the <see cref="LoadSecurementCheck"/> aggregate root.</summary>
public interface ILoadSecurementCheckRepository
{
    /// <summary>Retrieves a LoadSecurementCheck aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.LogisticsTransportSafety.LoadSecurementChecks.Aggregates.LoadSecurementCheck?> GetByIdAsync(LoadSecurementCheckId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all LoadSecurementCheck aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.LogisticsTransportSafety.LoadSecurementChecks.Aggregates.LoadSecurementCheck>> ListAsync(LoadSecurementCheckSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new LoadSecurementCheck aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.LogisticsTransportSafety.LoadSecurementChecks.Aggregates.LoadSecurementCheck aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing LoadSecurementCheck aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.LogisticsTransportSafety.LoadSecurementChecks.Aggregates.LoadSecurementCheck aggregate, CancellationToken cancellationToken = default);
}

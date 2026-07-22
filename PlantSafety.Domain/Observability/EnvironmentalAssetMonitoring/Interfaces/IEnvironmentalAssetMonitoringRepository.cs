// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.EnvironmentalAssetMonitoring
//  Layer:          Domain
//  File:           IEnvironmentalAssetMonitoringRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the EnvironmentalAssetMonitoring aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.EnvironmentalAssetMonitoring.Interfaces;

/// <summary>Domain repository interface for the <see cref="EnvironmentalAssetMonitoring"/> aggregate root.</summary>
public interface IEnvironmentalAssetMonitoringRepository
{
    /// <summary>Retrieves a EnvironmentalAssetMonitoring aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.Observability.EnvironmentalAssetMonitoring.Aggregates.EnvironmentalAssetMonitoring?> GetByIdAsync(EnvironmentalAssetMonitoringId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all EnvironmentalAssetMonitoring aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.Observability.EnvironmentalAssetMonitoring.Aggregates.EnvironmentalAssetMonitoring>> ListAsync(EnvironmentalAssetMonitoringSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new EnvironmentalAssetMonitoring aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.Observability.EnvironmentalAssetMonitoring.Aggregates.EnvironmentalAssetMonitoring aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing EnvironmentalAssetMonitoring aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.Observability.EnvironmentalAssetMonitoring.Aggregates.EnvironmentalAssetMonitoring aggregate, CancellationToken cancellationToken = default);
}
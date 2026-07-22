// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.EnvironmentalHealthMonitoring
//  Layer:          Domain
//  File:           IEnvironmentalHealthMonitoringRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the EnvironmentalHealthMonitoring aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.EnvironmentalHealthMonitoring.Interfaces;

/// <summary>Domain repository interface for the <see cref="EnvironmentalHealthMonitoring"/> aggregate root.</summary>
public interface IEnvironmentalHealthMonitoringRepository
{
    /// <summary>Retrieves a EnvironmentalHealthMonitoring aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.EmployeeSafety.EnvironmentalHealthMonitoring.Aggregates.EnvironmentalHealthMonitoring?> GetByIdAsync(EnvironmentalHealthMonitoringId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all EnvironmentalHealthMonitoring aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.EmployeeSafety.EnvironmentalHealthMonitoring.Aggregates.EnvironmentalHealthMonitoring>> ListAsync(EnvironmentalHealthMonitoringSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new EnvironmentalHealthMonitoring aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.EmployeeSafety.EnvironmentalHealthMonitoring.Aggregates.EnvironmentalHealthMonitoring aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing EnvironmentalHealthMonitoring aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.EmployeeSafety.EnvironmentalHealthMonitoring.Aggregates.EnvironmentalHealthMonitoring aggregate, CancellationToken cancellationToken = default);
}
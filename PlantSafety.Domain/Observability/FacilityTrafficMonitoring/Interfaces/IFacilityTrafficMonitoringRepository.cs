// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.FacilityTrafficMonitoring
//  Layer:          Domain
//  File:           IFacilityTrafficMonitoringRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the FacilityTrafficMonitoring aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.FacilityTrafficMonitoring.Interfaces;

/// <summary>Domain repository interface for the <see cref="FacilityTrafficMonitoring"/> aggregate root.</summary>
public interface IFacilityTrafficMonitoringRepository
{
    /// <summary>Retrieves a FacilityTrafficMonitoring aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.Observability.FacilityTrafficMonitoring.Aggregates.FacilityTrafficMonitoring?> GetByIdAsync(FacilityTrafficMonitoringId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all FacilityTrafficMonitoring aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.Observability.FacilityTrafficMonitoring.Aggregates.FacilityTrafficMonitoring>> ListAsync(FacilityTrafficMonitoringSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new FacilityTrafficMonitoring aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.Observability.FacilityTrafficMonitoring.Aggregates.FacilityTrafficMonitoring aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing FacilityTrafficMonitoring aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.Observability.FacilityTrafficMonitoring.Aggregates.FacilityTrafficMonitoring aggregate, CancellationToken cancellationToken = default);
}
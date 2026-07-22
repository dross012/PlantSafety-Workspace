// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.PPEBehavioralMonitoring
//  Layer:          Domain
//  File:           IPPEBehavioralMonitoringRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the PPEBehavioralMonitoring aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.PPEBehavioralMonitoring.Interfaces;

/// <summary>Domain repository interface for the <see cref="PPEBehavioralMonitoring"/> aggregate root.</summary>
public interface IPPEBehavioralMonitoringRepository
{
    /// <summary>Retrieves a PPEBehavioralMonitoring aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.Observability.PPEBehavioralMonitoring.Aggregates.PPEBehavioralMonitoring?> GetByIdAsync(PPEBehavioralMonitoringId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all PPEBehavioralMonitoring aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.Observability.PPEBehavioralMonitoring.Aggregates.PPEBehavioralMonitoring>> ListAsync(PPEBehavioralMonitoringSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new PPEBehavioralMonitoring aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.Observability.PPEBehavioralMonitoring.Aggregates.PPEBehavioralMonitoring aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing PPEBehavioralMonitoring aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.Observability.PPEBehavioralMonitoring.Aggregates.PPEBehavioralMonitoring aggregate, CancellationToken cancellationToken = default);
}
// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.ActiveMonitoringAuditing
//  Layer:          Domain
//  File:           IActiveMonitoringAuditingRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the ActiveMonitoringAuditing aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.ActiveMonitoringAuditing.Interfaces;

/// <summary>Domain repository interface for the <see cref="ActiveMonitoringAuditing"/> aggregate root.</summary>
public interface IActiveMonitoringAuditingRepository
{
    /// <summary>Retrieves a ActiveMonitoringAuditing aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.ContractorSafety.ActiveMonitoringAuditing.Aggregates.ActiveMonitoringAuditing?> GetByIdAsync(ActiveMonitoringAuditingId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all ActiveMonitoringAuditing aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.ContractorSafety.ActiveMonitoringAuditing.Aggregates.ActiveMonitoringAuditing>> ListAsync(ActiveMonitoringAuditingSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new ActiveMonitoringAuditing aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.ContractorSafety.ActiveMonitoringAuditing.Aggregates.ActiveMonitoringAuditing aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing ActiveMonitoringAuditing aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.ContractorSafety.ActiveMonitoringAuditing.Aggregates.ActiveMonitoringAuditing aggregate, CancellationToken cancellationToken = default);
}
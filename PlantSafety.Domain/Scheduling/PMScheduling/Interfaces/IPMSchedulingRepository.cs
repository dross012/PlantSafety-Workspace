// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PMScheduling
//  Layer:          Domain
//  File:           IPMSchedulingRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the PMScheduling aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.PMScheduling.Interfaces;

/// <summary>Domain repository interface for the <see cref="PMScheduling"/> aggregate root.</summary>
public interface IPMSchedulingRepository
{
    /// <summary>Retrieves a PMScheduling aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.Scheduling.PMScheduling.Aggregates.PMScheduling?> GetByIdAsync(PMSchedulingId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all PMScheduling aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.Scheduling.PMScheduling.Aggregates.PMScheduling>> ListAsync(PMSchedulingSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new PMScheduling aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.Scheduling.PMScheduling.Aggregates.PMScheduling aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing PMScheduling aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.Scheduling.PMScheduling.Aggregates.PMScheduling aggregate, CancellationToken cancellationToken = default);
}
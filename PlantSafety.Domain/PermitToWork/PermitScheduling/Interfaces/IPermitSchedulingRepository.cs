// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitScheduling
//  Layer:          Domain
//  File:           IPermitSchedulingRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the PermitScheduling aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PermitToWork.PermitScheduling.Interfaces;

/// <summary>Domain repository interface for the <see cref="PermitScheduling"/> aggregate root.</summary>
public interface IPermitSchedulingRepository
{
    /// <summary>Retrieves a PermitScheduling aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.PermitToWork.PermitScheduling.Aggregates.PermitScheduling?> GetByIdAsync(PermitSchedulingId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all PermitScheduling aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.PermitToWork.PermitScheduling.Aggregates.PermitScheduling>> ListAsync(PermitSchedulingSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new PermitScheduling aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.PermitToWork.PermitScheduling.Aggregates.PermitScheduling aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing PermitScheduling aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.PermitToWork.PermitScheduling.Aggregates.PermitScheduling aggregate, CancellationToken cancellationToken = default);
}
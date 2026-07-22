// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.PermitToWork
//  Layer:          Domain
//  File:           IPermitToWorkRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the PermitToWork aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.PermitToWork.Interfaces;

/// <summary>Domain repository interface for the <see cref="PermitToWork"/> aggregate root.</summary>
public interface IPermitToWorkRepository
{
    /// <summary>Retrieves a PermitToWork aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.ContractorSafety.PermitToWork.Aggregates.PermitToWork?> GetByIdAsync(PermitToWorkId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all PermitToWork aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.ContractorSafety.PermitToWork.Aggregates.PermitToWork>> ListAsync(PermitToWorkSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new PermitToWork aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.ContractorSafety.PermitToWork.Aggregates.PermitToWork aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing PermitToWork aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.ContractorSafety.PermitToWork.Aggregates.PermitToWork aggregate, CancellationToken cancellationToken = default);
}
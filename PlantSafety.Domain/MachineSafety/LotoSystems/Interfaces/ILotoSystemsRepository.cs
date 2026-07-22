// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.LotoSystems
//  Layer:          Domain
//  File:           ILotoSystemsRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the LotoSystems aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.LotoSystems.Interfaces;

/// <summary>Domain repository interface for the <see cref="LotoSystems"/> aggregate root.</summary>
public interface ILotoSystemsRepository
{
    /// <summary>Retrieves a LotoSystems aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.MachineSafety.LotoSystems.Aggregates.LotoSystems?> GetByIdAsync(LotoSystemsId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all LotoSystems aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.MachineSafety.LotoSystems.Aggregates.LotoSystems>> ListAsync(LotoSystemsSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new LotoSystems aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.MachineSafety.LotoSystems.Aggregates.LotoSystems aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing LotoSystems aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.MachineSafety.LotoSystems.Aggregates.LotoSystems aggregate, CancellationToken cancellationToken = default);
}
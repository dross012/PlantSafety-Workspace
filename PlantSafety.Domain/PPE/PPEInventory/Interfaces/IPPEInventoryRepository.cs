// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPEInventory
//  Layer:          Domain
//  File:           IPPEInventoryRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the PPEInventory aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPEInventory.Interfaces;

/// <summary>Domain repository interface for the <see cref="PPEInventory"/> aggregate root.</summary>
public interface IPPEInventoryRepository
{
    /// <summary>Retrieves a PPEInventory aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.PPE.PPEInventory.Aggregates.PPEInventory?> GetByIdAsync(PPEInventoryId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all PPEInventory aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.PPE.PPEInventory.Aggregates.PPEInventory>> ListAsync(PPEInventorySpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new PPEInventory aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.PPE.PPEInventory.Aggregates.PPEInventory aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing PPEInventory aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.PPE.PPEInventory.Aggregates.PPEInventory aggregate, CancellationToken cancellationToken = default);
}
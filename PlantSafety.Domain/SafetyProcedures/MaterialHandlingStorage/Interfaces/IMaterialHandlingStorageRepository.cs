// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.MaterialHandlingStorage
//  Layer:          Domain
//  File:           IMaterialHandlingStorageRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the MaterialHandlingStorage aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.MaterialHandlingStorage.Interfaces;

/// <summary>Domain repository interface for the <see cref="MaterialHandlingStorage"/> aggregate root.</summary>
public interface IMaterialHandlingStorageRepository
{
    /// <summary>Retrieves a MaterialHandlingStorage aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.SafetyProcedures.MaterialHandlingStorage.Aggregates.MaterialHandlingStorage?> GetByIdAsync(MaterialHandlingStorageId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all MaterialHandlingStorage aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.SafetyProcedures.MaterialHandlingStorage.Aggregates.MaterialHandlingStorage>> ListAsync(MaterialHandlingStorageSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new MaterialHandlingStorage aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.SafetyProcedures.MaterialHandlingStorage.Aggregates.MaterialHandlingStorage aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing MaterialHandlingStorage aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.SafetyProcedures.MaterialHandlingStorage.Aggregates.MaterialHandlingStorage aggregate, CancellationToken cancellationToken = default);
}
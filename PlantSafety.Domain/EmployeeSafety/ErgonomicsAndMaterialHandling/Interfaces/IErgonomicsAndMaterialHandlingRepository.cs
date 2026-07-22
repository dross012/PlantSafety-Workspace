// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.ErgonomicsAndMaterialHandling
//  Layer:          Domain
//  File:           IErgonomicsAndMaterialHandlingRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the ErgonomicsAndMaterialHandling aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.ErgonomicsAndMaterialHandling.Interfaces;

/// <summary>Domain repository interface for the <see cref="ErgonomicsAndMaterialHandling"/> aggregate root.</summary>
public interface IErgonomicsAndMaterialHandlingRepository
{
    /// <summary>Retrieves a ErgonomicsAndMaterialHandling aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.EmployeeSafety.ErgonomicsAndMaterialHandling.Aggregates.ErgonomicsAndMaterialHandling?> GetByIdAsync(ErgonomicsAndMaterialHandlingId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all ErgonomicsAndMaterialHandling aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.EmployeeSafety.ErgonomicsAndMaterialHandling.Aggregates.ErgonomicsAndMaterialHandling>> ListAsync(ErgonomicsAndMaterialHandlingSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new ErgonomicsAndMaterialHandling aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.EmployeeSafety.ErgonomicsAndMaterialHandling.Aggregates.ErgonomicsAndMaterialHandling aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing ErgonomicsAndMaterialHandling aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.EmployeeSafety.ErgonomicsAndMaterialHandling.Aggregates.ErgonomicsAndMaterialHandling aggregate, CancellationToken cancellationToken = default);
}
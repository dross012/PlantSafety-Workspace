// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.LotoPrograms
//  Layer:          Domain
//  File:           ILotoProgramsRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the LotoPrograms aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.LotoPrograms.Interfaces;

/// <summary>Domain repository interface for the <see cref="LotoPrograms"/> aggregate root.</summary>
public interface ILotoProgramsRepository
{
    /// <summary>Retrieves a LotoPrograms aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.EmployeeSafety.LotoPrograms.Aggregates.LotoPrograms?> GetByIdAsync(LotoProgramsId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all LotoPrograms aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.EmployeeSafety.LotoPrograms.Aggregates.LotoPrograms>> ListAsync(LotoProgramsSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new LotoPrograms aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.EmployeeSafety.LotoPrograms.Aggregates.LotoPrograms aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing LotoPrograms aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.EmployeeSafety.LotoPrograms.Aggregates.LotoPrograms aggregate, CancellationToken cancellationToken = default);
}
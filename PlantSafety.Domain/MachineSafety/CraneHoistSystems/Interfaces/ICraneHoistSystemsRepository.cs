// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.CraneHoistSystems
//  Layer:          Domain
//  File:           ICraneHoistSystemsRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain repository interface for the CraneHoistSystems aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.CraneHoistSystems.Interfaces;

/// <summary>Domain repository interface for the <see cref="CraneHoistSystems"/> aggregate root.</summary>
public interface ICraneHoistSystemsRepository
{
    /// <summary>Retrieves a CraneHoistSystems aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.MachineSafety.CraneHoistSystems.Aggregates.CraneHoistSystems?> GetByIdAsync(CraneHoistSystemsId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all CraneHoistSystems aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.MachineSafety.CraneHoistSystems.Aggregates.CraneHoistSystems>> ListAsync(CraneHoistSystemsSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new CraneHoistSystems aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.MachineSafety.CraneHoistSystems.Aggregates.CraneHoistSystems aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing CraneHoistSystems aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.MachineSafety.CraneHoistSystems.Aggregates.CraneHoistSystems aggregate, CancellationToken cancellationToken = default);
}

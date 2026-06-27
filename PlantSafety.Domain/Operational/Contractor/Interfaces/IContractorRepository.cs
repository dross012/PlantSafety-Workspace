// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Contractor
//  Layer:          Domain
//  File:           IContractorRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the Contractor aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.Contractor.Interfaces;

/// <summary>Domain repository interface for the <see cref="Contractor"/> aggregate root.</summary>
public interface IContractorRepository
{
    /// <summary>Retrieves a Contractor aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<Contractor?> GetByIdAsync(ContractorId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all Contractor aggregates matching the given specification.</summary>
    Task<IReadOnlyList<Contractor>> ListAsync(ContractorSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new Contractor aggregate.</summary>
    Task AddAsync(Contractor aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing Contractor aggregate.</summary>
    Task UpdateAsync(Contractor aggregate, CancellationToken cancellationToken = default);
}

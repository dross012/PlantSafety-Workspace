// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Permits
//  Layer:          Domain
//  File:           IPermitsRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the Permits aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Permits.Interfaces;

/// <summary>Domain repository interface for the <see cref="Permits"/> aggregate root.</summary>
public interface IPermitsRepository
{
    /// <summary>Retrieves a Permits aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<Permits?> GetByIdAsync(PermitsId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all Permits aggregates matching the given specification.</summary>
    Task<IReadOnlyList<Permits>> ListAsync(PermitsSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new Permits aggregate.</summary>
    Task AddAsync(Permits aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing Permits aggregate.</summary>
    Task UpdateAsync(Permits aggregate, CancellationToken cancellationToken = default);
}

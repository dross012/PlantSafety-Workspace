// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnterpriseFabric
//  Layer:          Domain
//  File:           IEnterpriseFabricRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the EnterpriseFabric aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnterpriseFabric.Repositories;

/// <summary>Domain repository interface for the <see cref="EnterpriseFabric"/> aggregate root.</summary>
public interface IEnterpriseFabricRepository
{
    /// <summary>Retrieves a EnterpriseFabric aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<EnterpriseFabric?> GetByIdAsync(EnterpriseFabricId id, CancellationToken cancellationToken = default);

    /// <summary>Persists a new EnterpriseFabric aggregate.</summary>
    Task AddAsync(EnterpriseFabric aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing EnterpriseFabric aggregate.</summary>
    Task UpdateAsync(EnterpriseFabric aggregate, CancellationToken cancellationToken = default);
}

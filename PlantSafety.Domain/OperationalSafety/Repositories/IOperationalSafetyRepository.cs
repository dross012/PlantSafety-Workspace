// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         OperationalSafety
//  Layer:          Domain
//  File:           IOperationalSafetyRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the OperationalSafety aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.OperationalSafety.Repositories;

/// <summary>Domain repository interface for the <see cref="OperationalSafety"/> aggregate root.</summary>
public interface IOperationalSafetyRepository
{
    /// <summary>Retrieves a OperationalSafety aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.OperationalSafety.Aggregates.OperationalSafety?> GetByIdAsync(OperationalSafetyId id, CancellationToken cancellationToken = default);

    /// <summary>Persists a new OperationalSafety aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.OperationalSafety.Aggregates.OperationalSafety aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing OperationalSafety aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.OperationalSafety.Aggregates.OperationalSafety aggregate, CancellationToken cancellationToken = default);
}

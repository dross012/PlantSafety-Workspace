// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Policy
//  Layer:          Domain
//  File:           IPolicyRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the Policy aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Policy.Interfaces;

/// <summary>Domain repository interface for the <see cref="Policy"/> aggregate root.</summary>
public interface IPolicyRepository
{
    /// <summary>Retrieves a Policy aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.Compliance.Policy.Aggregates.Policy?> GetByIdAsync(PolicyId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all Policy aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.Compliance.Policy.Aggregates.Policy>> ListAsync(PolicySpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new Policy aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.Compliance.Policy.Aggregates.Policy aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing Policy aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.Compliance.Policy.Aggregates.Policy aggregate, CancellationToken cancellationToken = default);
}

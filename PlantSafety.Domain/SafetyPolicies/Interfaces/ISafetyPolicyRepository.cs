// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyPolicies
//  Layer:          Domain
//  File:           ISafetyPolicyRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the SafetyPolicy aggregate root.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.SafetyPolicies.Aggregates;
using PlantSafety.Domain.SafetyPolicies.Specifications;
using PlantSafety.Domain.SafetyPolicies.ValueObjects.Ids;

namespace PlantSafety.Domain.SafetyPolicies.Interfaces;

/// <summary>Domain repository interface for the SafetyPolicy aggregate root.</summary>
public interface ISafetyPolicyRepository
{
    /// <summary>Retrieves a SafetyPolicy aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<SafetyPolicy?> GetByIdAsync(SafetyPolicyId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all SafetyPolicy aggregates matching the given specification.</summary>
    Task<IReadOnlyList<SafetyPolicy>> ListAsync(SafetyPolicySpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new SafetyPolicy aggregate.</summary>
    Task AddAsync(SafetyPolicy aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing SafetyPolicy aggregate.</summary>
    Task UpdateAsync(SafetyPolicy aggregate, CancellationToken cancellationToken = default);
}
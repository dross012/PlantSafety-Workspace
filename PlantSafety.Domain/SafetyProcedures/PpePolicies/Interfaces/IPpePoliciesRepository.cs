// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.PpePolicies
//  Layer:          Domain
//  File:           IPpePoliciesRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the PpePolicies aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.PpePolicies.Interfaces;

/// <summary>Domain repository interface for the <see cref="PpePolicies"/> aggregate root.</summary>
public interface IPpePoliciesRepository
{
    /// <summary>Retrieves a PpePolicies aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.SafetyProcedures.PpePolicies.Aggregates.PpePolicies?> GetByIdAsync(PpePoliciesId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all PpePolicies aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.SafetyProcedures.PpePolicies.Aggregates.PpePolicies>> ListAsync(PpePoliciesSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new PpePolicies aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.SafetyProcedures.PpePolicies.Aggregates.PpePolicies aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing PpePolicies aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.SafetyProcedures.PpePolicies.Aggregates.PpePolicies aggregate, CancellationToken cancellationToken = default);
}
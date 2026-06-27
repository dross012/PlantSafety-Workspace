// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Governance
//  Layer:          Domain
//  File:           IGovernanceRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the Governance aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Governance.Repositories;

/// <summary>Domain repository interface for the <see cref="Governance"/> aggregate root.</summary>
public interface IGovernanceRepository
{
    /// <summary>Retrieves a Governance aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<Governance?> GetByIdAsync(GovernanceId id, CancellationToken cancellationToken = default);

    /// <summary>Persists a new Governance aggregate.</summary>
    Task AddAsync(Governance aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing Governance aggregate.</summary>
    Task UpdateAsync(Governance aggregate, CancellationToken cancellationToken = default);
}

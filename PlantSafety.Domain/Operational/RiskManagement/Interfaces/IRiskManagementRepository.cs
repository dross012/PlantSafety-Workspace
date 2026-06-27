// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.RiskManagement
//  Layer:          Domain
//  File:           IRiskManagementRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the RiskManagement aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.RiskManagement.Interfaces;

/// <summary>Domain repository interface for the <see cref="RiskManagement"/> aggregate root.</summary>
public interface IRiskManagementRepository
{
    /// <summary>Retrieves a RiskManagement aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<RiskManagement?> GetByIdAsync(RiskManagementId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all RiskManagement aggregates matching the given specification.</summary>
    Task<IReadOnlyList<RiskManagement>> ListAsync(RiskManagementSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new RiskManagement aggregate.</summary>
    Task AddAsync(RiskManagement aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing RiskManagement aggregate.</summary>
    Task UpdateAsync(RiskManagement aggregate, CancellationToken cancellationToken = default);
}

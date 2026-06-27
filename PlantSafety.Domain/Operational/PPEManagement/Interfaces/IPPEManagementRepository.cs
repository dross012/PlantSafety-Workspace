// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PPEManagement
//  Layer:          Domain
//  File:           IPPEManagementRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the PPEManagement aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.PPEManagement.Interfaces;

/// <summary>Domain repository interface for the <see cref="PPEManagement"/> aggregate root.</summary>
public interface IPPEManagementRepository
{
    /// <summary>Retrieves a PPEManagement aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PPEManagement?> GetByIdAsync(PPEManagementId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all PPEManagement aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PPEManagement>> ListAsync(PPEManagementSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new PPEManagement aggregate.</summary>
    Task AddAsync(PPEManagement aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing PPEManagement aggregate.</summary>
    Task UpdateAsync(PPEManagement aggregate, CancellationToken cancellationToken = default);
}

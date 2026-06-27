// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.ManagementOfChange
//  Layer:          Domain
//  File:           IManagementOfChangeRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the ManagementOfChange aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.ManagementOfChange.Interfaces;

/// <summary>Domain repository interface for the <see cref="ManagementOfChange"/> aggregate root.</summary>
public interface IManagementOfChangeRepository
{
    /// <summary>Retrieves a ManagementOfChange aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<ManagementOfChange?> GetByIdAsync(ManagementOfChangeId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all ManagementOfChange aggregates matching the given specification.</summary>
    Task<IReadOnlyList<ManagementOfChange>> ListAsync(ManagementOfChangeSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new ManagementOfChange aggregate.</summary>
    Task AddAsync(ManagementOfChange aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing ManagementOfChange aggregate.</summary>
    Task UpdateAsync(ManagementOfChange aggregate, CancellationToken cancellationToken = default);
}

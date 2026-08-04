// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.AccessAnomalies
//  Layer:          Domain
//  File:           IAccessAnomalyRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the AccessAnomaly aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.Interfaces;

/// <summary>Domain repository interface for the <see cref="AccessAnomaly"/> aggregate root.</summary>
public interface IAccessAnomalyRepository
{
    /// <summary>Retrieves an AccessAnomaly aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.Aggregates.AccessAnomaly?> GetByIdAsync(AccessAnomalyId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all AccessAnomaly aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.Aggregates.AccessAnomaly>> ListAsync(AccessAnomalySpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new AccessAnomaly aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.Aggregates.AccessAnomaly aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing AccessAnomaly aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.Aggregates.AccessAnomaly aggregate, CancellationToken cancellationToken = default);
}

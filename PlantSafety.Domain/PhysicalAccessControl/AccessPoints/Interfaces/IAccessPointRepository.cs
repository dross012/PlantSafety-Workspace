// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.AccessPoints
//  Layer:          Domain
//  File:           IAccessPointRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the AccessPoint aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.AccessPoints.Interfaces;

/// <summary>Domain repository interface for the <see cref="AccessPoint"/> aggregate root.</summary>
public interface IAccessPointRepository
{
    /// <summary>Retrieves an AccessPoint aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.PhysicalAccessControl.AccessPoints.Aggregates.AccessPoint?> GetByIdAsync(AccessPointId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all AccessPoint aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.PhysicalAccessControl.AccessPoints.Aggregates.AccessPoint>> ListAsync(AccessPointSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new AccessPoint aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.PhysicalAccessControl.AccessPoints.Aggregates.AccessPoint aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing AccessPoint aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.PhysicalAccessControl.AccessPoints.Aggregates.AccessPoint aggregate, CancellationToken cancellationToken = default);
}

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.AccessEvents
//  Layer:          Domain
//  File:           IAccessEventRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the AccessEvent aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.AccessEvents.Interfaces;

/// <summary>Domain repository interface for the <see cref="AccessEvent"/> aggregate root.</summary>
public interface IAccessEventRepository
{
    /// <summary>Retrieves an AccessEvent aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.PhysicalAccessControl.AccessEvents.Aggregates.AccessEvent?> GetByIdAsync(AccessEventId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all AccessEvent aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.PhysicalAccessControl.AccessEvents.Aggregates.AccessEvent>> ListAsync(AccessEventSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new AccessEvent aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.PhysicalAccessControl.AccessEvents.Aggregates.AccessEvent aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing AccessEvent aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.PhysicalAccessControl.AccessEvents.Aggregates.AccessEvent aggregate, CancellationToken cancellationToken = default);
}

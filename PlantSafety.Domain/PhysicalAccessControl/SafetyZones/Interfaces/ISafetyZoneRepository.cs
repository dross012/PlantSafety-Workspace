// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.SafetyZones
//  Layer:          Domain
//  File:           ISafetyZoneRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the SafetyZone aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.SafetyZones.Interfaces;

/// <summary>Domain repository interface for the <see cref="SafetyZone"/> aggregate root.</summary>
public interface ISafetyZoneRepository
{
    /// <summary>Retrieves a SafetyZone aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.PhysicalAccessControl.SafetyZones.Aggregates.SafetyZone?> GetByIdAsync(SafetyZoneId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all SafetyZone aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.PhysicalAccessControl.SafetyZones.Aggregates.SafetyZone>> ListAsync(SafetyZoneSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new SafetyZone aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.PhysicalAccessControl.SafetyZones.Aggregates.SafetyZone aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing SafetyZone aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.PhysicalAccessControl.SafetyZones.Aggregates.SafetyZone aggregate, CancellationToken cancellationToken = default);
}

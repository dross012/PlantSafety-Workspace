// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.RestrictedZonePolicies
//  Layer:          Domain
//  File:           IRestrictedZonePolicyRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the RestrictedZonePolicy aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.RestrictedZonePolicies.Interfaces;

/// <summary>Domain repository interface for the <see cref="RestrictedZonePolicy"/> aggregate root.</summary>
public interface IRestrictedZonePolicyRepository
{
    /// <summary>Retrieves a RestrictedZonePolicy aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.PhysicalAccessControl.RestrictedZonePolicies.Aggregates.RestrictedZonePolicy?> GetByIdAsync(RestrictedZonePolicyId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all RestrictedZonePolicy aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.PhysicalAccessControl.RestrictedZonePolicies.Aggregates.RestrictedZonePolicy>> ListAsync(RestrictedZonePolicySpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new RestrictedZonePolicy aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.PhysicalAccessControl.RestrictedZonePolicies.Aggregates.RestrictedZonePolicy aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing RestrictedZonePolicy aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.PhysicalAccessControl.RestrictedZonePolicies.Aggregates.RestrictedZonePolicy aggregate, CancellationToken cancellationToken = default);
}

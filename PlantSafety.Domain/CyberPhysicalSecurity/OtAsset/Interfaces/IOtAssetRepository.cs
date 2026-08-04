// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.OtAsset
//  Layer:          Domain
//  File:           IOtAssetRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the OtAsset aggregate root.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.Specifications;
using PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.ValueObjects.Ids;

namespace PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.Interfaces;

/// <summary>Domain repository interface for the <see cref="Aggregates.OtAsset"/> aggregate root.</summary>
public interface IOtAssetRepository
{
    /// <summary>Retrieves an OtAsset by its identity, or <see langword="null"/> if not found.</summary>
    Task<Aggregates.OtAsset?> GetByIdAsync(OtAssetId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Returns all OtAssets matching the given specification.</summary>
    Task<IReadOnlyList<Aggregates.OtAsset>> ListAsync(Guid tenantId, OtAssetSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new OtAsset.</summary>
    Task AddAsync(Aggregates.OtAsset aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing OtAsset.</summary>
    Task UpdateAsync(Aggregates.OtAsset aggregate, CancellationToken cancellationToken = default);
}

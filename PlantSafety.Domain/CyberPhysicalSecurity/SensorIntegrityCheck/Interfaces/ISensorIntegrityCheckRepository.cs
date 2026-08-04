// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.SensorIntegrityCheck
//  Layer:          Domain
//  File:           ISensorIntegrityCheckRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the SensorIntegrityCheck aggregate root.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.ValueObjects.Ids;
using PlantSafety.Domain.CyberPhysicalSecurity.SensorIntegrityCheck.ValueObjects.Ids;

namespace PlantSafety.Domain.CyberPhysicalSecurity.SensorIntegrityCheck.Interfaces;

/// <summary>Domain repository interface for the <see cref="Aggregates.SensorIntegrityCheck"/> aggregate root.</summary>
public interface ISensorIntegrityCheckRepository
{
    /// <summary>Retrieves a SensorIntegrityCheck by its identity, or <see langword="null"/> if not found.</summary>
    Task<Aggregates.SensorIntegrityCheck?> GetByIdAsync(SensorIntegrityCheckId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Returns the full integrity-check history for a given OT asset, most recent first.</summary>
    Task<IReadOnlyList<Aggregates.SensorIntegrityCheck>> GetHistoryForAssetAsync(OtAssetId otAssetId, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Returns OT assets with a recurring integrity check due at or before <paramref name="asOfUtc"/> (scheduling hook).</summary>
    Task<IReadOnlyList<OtAssetId>> ListAssetsWithChecksDueAsync(Guid tenantId, DateTime asOfUtc, CancellationToken cancellationToken = default);

    /// <summary>Persists a new SensorIntegrityCheck record.</summary>
    Task AddAsync(Aggregates.SensorIntegrityCheck aggregate, CancellationToken cancellationToken = default);
}

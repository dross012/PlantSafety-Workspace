// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.FeatureEngineering
//  Layer:          Application
//  File:           IFeatureSetDetailRepository.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
//  SchemaVersion:  2026.1
//  Description:    Read-only repository abstraction for a single FeatureSet record by identity,
//                  backing GetFeatureSetByIdQuery. Implemented in the Infrastructure layer by
//                  FeatureSetDetailDapperRepository. Mirrors
//                  PlantSafety.Application.Intelligence.DataLake.Repositories
//                  .IDataLakeActivityDetailRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Intelligence.FeatureEngineering.Models;

namespace PlantSafety.Application.Intelligence.FeatureEngineering.Repositories;

/// <summary>Read-only repository for a single FeatureSet record by identity. Implemented in the Infrastructure layer.</summary>
public interface IFeatureSetDetailRepository
{
    /// <summary>
    /// Returns the full FeatureSet read-model record for the given identity, scoped to the given
    /// tenant, or <see langword="null"/> if not found or if it belongs to a different tenant.
    /// </summary>
    Task<FeatureSetDetailDto?> GetByIdAsync(Guid featureSetId, Guid tenantId, CancellationToken cancellationToken);
}

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.ModelRegistry
//  Layer:          Application
//  File:           IModelRegistryEntryDetailRepository.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
//  SchemaVersion:  2026.1
//  Description:    Read-only repository abstraction for a single ModelRegistryEntry record by
//                  identity, backing GetModelRegistryEntryByIdQuery. Implemented in the
//                  Infrastructure layer by ModelRegistryEntryDetailDapperRepository. Mirrors
//                  PlantSafety.Application.Intelligence.DataLake.Repositories
//                  .IDataLakeActivityDetailRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Intelligence.ModelRegistry.Models;

namespace PlantSafety.Application.Intelligence.ModelRegistry.Repositories;

/// <summary>Read-only repository for a single ModelRegistryEntry record by identity. Implemented in the Infrastructure layer.</summary>
public interface IModelRegistryEntryDetailRepository
{
    /// <summary>
    /// Returns the full ModelRegistryEntry read-model record for the given identity, scoped to
    /// the given tenant, or <see langword="null"/> if not found or if it belongs to a different
    /// tenant.
    /// </summary>
    Task<ModelRegistryEntryDetailDto?> GetByIdAsync(Guid modelRegistryEntryId, Guid tenantId, CancellationToken cancellationToken);
}

// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the Material Handling & Storage Policies read-model.
// SchemaVersion: 1

using PlantSafety.Application.SafetyPolicies.MaterialHandlingStorage.Models;

namespace PlantSafety.Application.SafetyPolicies.MaterialHandlingStorage.Repositories;

/// <summary>Read-only repository for Material Handling &amp; Storage Policies data. Implemented in the Infrastructure layer.</summary>
public interface IMaterialHandlingStorageRepository
{
    /// <summary>Returns Material Handling &amp; Storage Policies items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<MaterialHandlingStorageItemDto>> GetMaterialHandlingStorageAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
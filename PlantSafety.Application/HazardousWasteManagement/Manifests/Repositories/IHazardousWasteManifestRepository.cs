// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Repository abstraction for the HazardousWasteManifest read-model.
// SchemaVersion: 1

using PlantSafety.Application.HazardousWasteManagement.Manifests.Models;

namespace PlantSafety.Application.HazardousWasteManagement.Manifests.Repositories;

/// <summary>Read-only repository for HazardousWasteManifest data. Implemented in the Infrastructure layer.</summary>
public interface IHazardousWasteManifestRepository
{
    /// <summary>Returns a single manifest item by identifier, or null if not found.</summary>
    Task<HazardousWasteManifestItemDto?> GetManifestByIdAsync(Guid tenantId, Guid manifestId, CancellationToken cancellationToken);

    /// <summary>Returns all manifests that are not yet Closed (Generated, InTransit, ReceivedAtTsdf, DiscrepancyFlagged).</summary>
    Task<IReadOnlyList<HazardousWasteManifestItemDto>> ListOpenManifestsAsync(Guid tenantId, Guid? siteId, CancellationToken cancellationToken);
}

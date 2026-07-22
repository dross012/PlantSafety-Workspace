// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the PPECategory read-model.
// SchemaVersion: 1

using PlantSafety.Application.PPE.PPECategory.Models;

namespace PlantSafety.Application.PPE.PPECategory.Repositories;

/// <summary>Read-only repository for PPECategory data. Implemented in the Infrastructure layer.</summary>
public interface IPPECategoryRepository
{
    /// <summary>Returns PPECategory items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<PPECategoryItemDto>> GetPPECategoryAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
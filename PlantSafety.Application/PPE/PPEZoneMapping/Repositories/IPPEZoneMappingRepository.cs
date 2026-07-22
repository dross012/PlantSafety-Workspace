// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the PPEZoneMapping read-model.
// SchemaVersion: 1

using PlantSafety.Application.PPE.PPEZoneMapping.Models;

namespace PlantSafety.Application.PPE.PPEZoneMapping.Repositories;

/// <summary>Read-only repository for PPEZoneMapping data. Implemented in the Infrastructure layer.</summary>
public interface IPPEZoneMappingRepository
{
    /// <summary>Returns PPEZoneMapping items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<PPEZoneMappingItemDto>> GetPPEZoneMappingAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
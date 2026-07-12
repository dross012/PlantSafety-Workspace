// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the HotTap read-model.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Permits.HotTap.Models;

namespace PlantSafety.Application.Compliance.Permits.HotTap.Repositories;

/// <summary>Read-only repository for HotTap data. Implemented in the Infrastructure layer.</summary>
public interface IHotTapRepository
{
    /// <summary>Returns HotTap items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<HotTapItemDto>> GetHotTapAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}

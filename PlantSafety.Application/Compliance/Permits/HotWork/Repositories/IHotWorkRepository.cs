// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the HotWork read-model.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Permits.HotWork.Models;

namespace PlantSafety.Application.Compliance.Permits.HotWork.Repositories;

/// <summary>Read-only repository for HotWork data. Implemented in the Infrastructure layer.</summary>
public interface IHotWorkRepository
{
    /// <summary>Returns HotWork items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<HotWorkItemDto>> GetHotWorkAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}

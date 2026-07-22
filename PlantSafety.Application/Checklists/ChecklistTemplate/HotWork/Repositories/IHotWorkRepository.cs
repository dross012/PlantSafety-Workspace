// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the HotWork read-model. Implemented in the Infrastructure layer (Dapper/stored-procedure-backed; not generated in this C#-only pass).
// SchemaVersion: 1

using PlantSafety.Application.Checklists.ChecklistTemplate.HotWork.Models;

namespace PlantSafety.Application.Checklists.ChecklistTemplate.HotWork.Repositories;

/// <summary>Read-only repository for HotWork data. Implemented in the Infrastructure layer.</summary>
public interface IHotWorkRepository
{
    /// <summary>Returns HotWork items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<HotWorkItemDto>> GetItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
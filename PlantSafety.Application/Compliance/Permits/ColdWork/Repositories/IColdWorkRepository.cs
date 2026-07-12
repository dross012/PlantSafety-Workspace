// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the ColdWork read-model.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Permits.ColdWork.Models;

namespace PlantSafety.Application.Compliance.Permits.ColdWork.Repositories;

/// <summary>Read-only repository for ColdWork data. Implemented in the Infrastructure layer.</summary>
public interface IColdWorkRepository
{
    /// <summary>Returns ColdWork items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<ColdWorkItemDto>> GetColdWorkAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}

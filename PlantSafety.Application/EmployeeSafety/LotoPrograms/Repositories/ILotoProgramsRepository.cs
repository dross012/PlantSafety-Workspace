// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the LotoPrograms read-model.
// SchemaVersion: 1

using PlantSafety.Application.EmployeeSafety.LotoPrograms.Models;

namespace PlantSafety.Application.EmployeeSafety.LotoPrograms.Repositories;

/// <summary>Read-only repository for LotoPrograms data. Implemented in the Infrastructure layer.</summary>
public interface ILotoProgramsRepository
{
    /// <summary>Returns LotoPrograms items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<LotoProgramsItemDto>> GetLotoProgramsItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
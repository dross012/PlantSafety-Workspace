// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the ErgonomicsAndMaterialHandling read-model.
// SchemaVersion: 1

using PlantSafety.Application.EmployeeSafety.ErgonomicsAndMaterialHandling.Models;

namespace PlantSafety.Application.EmployeeSafety.ErgonomicsAndMaterialHandling.Repositories;

/// <summary>Read-only repository for ErgonomicsAndMaterialHandling data. Implemented in the Infrastructure layer.</summary>
public interface IErgonomicsAndMaterialHandlingRepository
{
    /// <summary>Returns ErgonomicsAndMaterialHandling items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<ErgonomicsAndMaterialHandlingItemDto>> GetErgonomicsAndMaterialHandlingItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
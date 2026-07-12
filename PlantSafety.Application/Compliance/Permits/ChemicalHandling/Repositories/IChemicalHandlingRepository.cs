// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the ChemicalHandling read-model.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Permits.ChemicalHandling.Models;

namespace PlantSafety.Application.Compliance.Permits.ChemicalHandling.Repositories;

/// <summary>Read-only repository for ChemicalHandling data. Implemented in the Infrastructure layer.</summary>
public interface IChemicalHandlingRepository
{
    /// <summary>Returns ChemicalHandling items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<ChemicalHandlingItemDto>> GetChemicalHandlingAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}

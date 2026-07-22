// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the Chemical & Environmental Safety read-model.
// SchemaVersion: 1

using PlantSafety.Application.RiskManagement.ChemicalEnvironmentalSafety.Models;

namespace PlantSafety.Application.RiskManagement.ChemicalEnvironmentalSafety.Repositories;

/// <summary>Read-only repository for Chemical & Environmental Safety data. Implemented in the Infrastructure layer.</summary>
public interface IChemicalEnvironmentalSafetyRepository
{
    /// <summary>Returns Chemical & Environmental Safety items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<ChemicalEnvironmentalSafetyItemDto>> GetChemicalEnvironmentalSafetyAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
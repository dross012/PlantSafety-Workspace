// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the HazardousMaterialsAndChemicalSafety read-model. Implemented in the Infrastructure layer (Dapper/stored-procedure-backed; not generated in this C#-only pass).
// SchemaVersion: 1

using PlantSafety.Application.Checklists.ChecklistTemplate.HazardousMaterialsAndChemicalSafety.Models;

namespace PlantSafety.Application.Checklists.ChecklistTemplate.HazardousMaterialsAndChemicalSafety.Repositories;

/// <summary>Read-only repository for HazardousMaterialsAndChemicalSafety data. Implemented in the Infrastructure layer.</summary>
public interface IHazardousMaterialsAndChemicalSafetyRepository
{
    /// <summary>Returns HazardousMaterialsAndChemicalSafety items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<HazardousMaterialsAndChemicalSafetyItemDto>> GetItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
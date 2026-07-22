// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the ContractorSafety read-model. Implemented in the Infrastructure layer (Dapper/stored-procedure-backed; not generated in this C#-only pass).
// SchemaVersion: 1

using PlantSafety.Application.Checklists.ChecklistTemplate.ContractorSafety.Models;

namespace PlantSafety.Application.Checklists.ChecklistTemplate.ContractorSafety.Repositories;

/// <summary>Read-only repository for ContractorSafety data. Implemented in the Infrastructure layer.</summary>
public interface IContractorSafetyRepository
{
    /// <summary>Returns ContractorSafety items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<ContractorSafetyItemDto>> GetItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
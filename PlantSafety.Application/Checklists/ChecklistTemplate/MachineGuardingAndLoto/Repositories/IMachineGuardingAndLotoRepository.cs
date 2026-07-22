// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the MachineGuardingAndLoto read-model. Implemented in the Infrastructure layer (Dapper/stored-procedure-backed; not generated in this C#-only pass).
// SchemaVersion: 1

using PlantSafety.Application.Checklists.ChecklistTemplate.MachineGuardingAndLoto.Models;

namespace PlantSafety.Application.Checklists.ChecklistTemplate.MachineGuardingAndLoto.Repositories;

/// <summary>Read-only repository for MachineGuardingAndLoto data. Implemented in the Infrastructure layer.</summary>
public interface IMachineGuardingAndLotoRepository
{
    /// <summary>Returns MachineGuardingAndLoto items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<MachineGuardingAndLotoItemDto>> GetItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
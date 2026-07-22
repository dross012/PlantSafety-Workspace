// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the MachineGuardsBarriers read-model.
// SchemaVersion: 1

using PlantSafety.Application.MachineSafety.MachineGuardsBarriers.Models;

namespace PlantSafety.Application.MachineSafety.MachineGuardsBarriers.Repositories;

/// <summary>Read-only repository for MachineGuardsBarriers data. Implemented in the Infrastructure layer.</summary>
public interface IMachineGuardsBarriersRepository
{
    /// <summary>Returns MachineGuardsBarriers items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<MachineGuardsBarriersItemDto>> GetMachineGuardsBarriersItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
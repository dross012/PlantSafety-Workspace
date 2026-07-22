// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the MachineGuardingInspections read-model.
// SchemaVersion: 1

using PlantSafety.Application.SafetyProcedures.MachineGuardingInspections.Models;

namespace PlantSafety.Application.SafetyProcedures.MachineGuardingInspections.Repositories;

/// <summary>Read-only repository for MachineGuardingInspections data. Implemented in the Infrastructure layer.</summary>
public interface IMachineGuardingInspectionsRepository
{
    /// <summary>Returns MachineGuardingInspections items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<MachineGuardingInspectionsItemDto>> GetMachineGuardingInspectionsItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
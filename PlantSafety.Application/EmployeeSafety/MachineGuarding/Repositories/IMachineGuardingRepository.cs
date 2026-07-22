// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the MachineGuarding read-model.
// SchemaVersion: 1

using PlantSafety.Application.EmployeeSafety.MachineGuarding.Models;

namespace PlantSafety.Application.EmployeeSafety.MachineGuarding.Repositories;

/// <summary>Read-only repository for MachineGuarding data. Implemented in the Infrastructure layer.</summary>
public interface IMachineGuardingRepository
{
    /// <summary>Returns MachineGuarding items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<MachineGuardingItemDto>> GetMachineGuardingItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
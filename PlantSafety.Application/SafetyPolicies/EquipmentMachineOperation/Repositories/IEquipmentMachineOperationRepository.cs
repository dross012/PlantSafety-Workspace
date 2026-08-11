// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the Equipment & Machine Operation Policies read-model.
// SchemaVersion: 1

using PlantSafety.Application.SafetyPolicies.EquipmentMachineOperation.Models;

namespace PlantSafety.Application.SafetyPolicies.EquipmentMachineOperation.Repositories;

/// <summary>Read-only repository for Equipment &amp; Machine Operation Policies data. Implemented in the Infrastructure layer.</summary>
public interface IEquipmentMachineOperationRepository
{
    /// <summary>Returns Equipment &amp; Machine Operation Policies items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<EquipmentMachineOperationItemDto>> GetEquipmentMachineOperationAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
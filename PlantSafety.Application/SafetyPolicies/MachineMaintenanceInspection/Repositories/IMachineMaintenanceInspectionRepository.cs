// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the Machine Maintenance & Inspection Policies read-model.
// SchemaVersion: 1

using PlantSafety.Application.SafetyPolicies.MachineMaintenanceInspection.Models;

namespace PlantSafety.Application.SafetyPolicies.MachineMaintenanceInspection.Repositories;

/// <summary>Read-only repository for Machine Maintenance & Inspection Policies data. Implemented in the Infrastructure layer.</summary>
public interface IMachineMaintenanceInspectionRepository
{
    /// <summary>Returns Machine Maintenance & Inspection Policies items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<MachineMaintenanceInspectionItemDto>> GetMachineMaintenanceInspectionAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
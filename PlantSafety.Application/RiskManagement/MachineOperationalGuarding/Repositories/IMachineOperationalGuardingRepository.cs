// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the Machine & Operational Guarding read-model.
// SchemaVersion: 1

using PlantSafety.Application.RiskManagement.MachineOperationalGuarding.Models;

namespace PlantSafety.Application.RiskManagement.MachineOperationalGuarding.Repositories;

/// <summary>Read-only repository for Machine &amp; Operational Guarding data. Implemented in the Infrastructure layer.</summary>
public interface IMachineOperationalGuardingRepository
{
    /// <summary>Returns Machine &amp; Operational Guarding items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<MachineOperationalGuardingItemDto>> GetMachineOperationalGuardingAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the Electrical Safety Policies read-model.
// SchemaVersion: 1

using PlantSafety.Application.SafetyPolicies.ElectricalSafety.Models;

namespace PlantSafety.Application.SafetyPolicies.ElectricalSafety.Repositories;

/// <summary>Read-only repository for Electrical Safety Policies data. Implemented in the Infrastructure layer.</summary>
public interface IElectricalSafetyRepository
{
    /// <summary>Returns Electrical Safety Policies items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<ElectricalSafetyItemDto>> GetElectricalSafetyAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the Emergency Response & Evacuation Policies read-model.
// SchemaVersion: 1

using PlantSafety.Application.SafetyPolicies.EmergencyResponseEvacuation.Models;

namespace PlantSafety.Application.SafetyPolicies.EmergencyResponseEvacuation.Repositories;

/// <summary>Read-only repository for Emergency Response &amp; Evacuation Policies data. Implemented in the Infrastructure layer.</summary>
public interface IEmergencyResponseEvacuationRepository
{
    /// <summary>Returns Emergency Response &amp; Evacuation Policies items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<EmergencyResponseEvacuationItemDto>> GetEmergencyResponseEvacuationAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
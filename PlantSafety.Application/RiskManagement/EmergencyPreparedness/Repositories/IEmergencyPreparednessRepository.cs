// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the Emergency Preparedness read-model.
// SchemaVersion: 1

using PlantSafety.Application.RiskManagement.EmergencyPreparedness.Models;

namespace PlantSafety.Application.RiskManagement.EmergencyPreparedness.Repositories;

/// <summary>Read-only repository for Emergency Preparedness data. Implemented in the Infrastructure layer.</summary>
public interface IEmergencyPreparednessRepository
{
    /// <summary>Returns Emergency Preparedness items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<EmergencyPreparednessItemDto>> GetEmergencyPreparednessAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
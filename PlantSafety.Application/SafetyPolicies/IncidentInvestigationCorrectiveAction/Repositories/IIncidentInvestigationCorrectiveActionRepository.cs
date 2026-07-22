// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the Incident Investigation & Corrective Action Policies read-model.
// SchemaVersion: 1

using PlantSafety.Application.SafetyPolicies.IncidentInvestigationCorrectiveAction.Models;

namespace PlantSafety.Application.SafetyPolicies.IncidentInvestigationCorrectiveAction.Repositories;

/// <summary>Read-only repository for Incident Investigation & Corrective Action Policies data. Implemented in the Infrastructure layer.</summary>
public interface IIncidentInvestigationCorrectiveActionRepository
{
    /// <summary>Returns Incident Investigation & Corrective Action Policies items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<IncidentInvestigationCorrectiveActionItemDto>> GetIncidentInvestigationCorrectiveActionAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
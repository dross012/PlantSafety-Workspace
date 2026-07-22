// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the Working at Heights Policies read-model.
// SchemaVersion: 1

using PlantSafety.Application.SafetyPolicies.WorkingAtHeights.Models;

namespace PlantSafety.Application.SafetyPolicies.WorkingAtHeights.Repositories;

/// <summary>Read-only repository for Working at Heights Policies data. Implemented in the Infrastructure layer.</summary>
public interface IWorkingAtHeightsRepository
{
    /// <summary>Returns Working at Heights Policies items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<WorkingAtHeightsItemDto>> GetWorkingAtHeightsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the Hot Work Policies read-model.
// SchemaVersion: 1

using PlantSafety.Application.SafetyPolicies.HotWork.Models;

namespace PlantSafety.Application.SafetyPolicies.HotWork.Repositories;

/// <summary>Read-only repository for Hot Work Policies data. Implemented in the Infrastructure layer.</summary>
public interface IHotWorkRepository
{
    /// <summary>Returns Hot Work Policies items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<HotWorkItemDto>> GetHotWorkAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
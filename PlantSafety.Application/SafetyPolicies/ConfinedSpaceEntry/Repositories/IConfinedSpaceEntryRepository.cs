// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the Confined Space Entry Policies read-model.
// SchemaVersion: 1

using PlantSafety.Application.SafetyPolicies.ConfinedSpaceEntry.Models;

namespace PlantSafety.Application.SafetyPolicies.ConfinedSpaceEntry.Repositories;

/// <summary>Read-only repository for Confined Space Entry Policies data. Implemented in the Infrastructure layer.</summary>
public interface IConfinedSpaceEntryRepository
{
    /// <summary>Returns Confined Space Entry Policies items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<ConfinedSpaceEntryItemDto>> GetConfinedSpaceEntryAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
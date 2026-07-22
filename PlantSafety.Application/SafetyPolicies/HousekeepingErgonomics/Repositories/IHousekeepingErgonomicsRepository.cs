// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the Housekeeping & Ergonomics Policies read-model.
// SchemaVersion: 1

using PlantSafety.Application.SafetyPolicies.HousekeepingErgonomics.Models;

namespace PlantSafety.Application.SafetyPolicies.HousekeepingErgonomics.Repositories;

/// <summary>Read-only repository for Housekeeping & Ergonomics Policies data. Implemented in the Infrastructure layer.</summary>
public interface IHousekeepingErgonomicsRepository
{
    /// <summary>Returns Housekeeping & Ergonomics Policies items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<HousekeepingErgonomicsItemDto>> GetHousekeepingErgonomicsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
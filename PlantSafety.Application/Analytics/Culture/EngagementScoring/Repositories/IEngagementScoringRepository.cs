// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the EngagementScoring read-model.
// SchemaVersion: 1

using PlantSafety.Application.Analytics.Culture.EngagementScoring.Models;

namespace PlantSafety.Application.Analytics.Culture.EngagementScoring.Repositories;

/// <summary>Read-only repository for EngagementScoring data. Implemented in the Infrastructure layer.</summary>
public interface IEngagementScoringRepository
{
    /// <summary>Returns EngagementScoring items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<EngagementScoringItemDto>> GetEngagementScoresAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}

// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the LeadershipEngagementFrequency read-model. Implemented in the Infrastructure layer (Dapper/stored-procedure-backed; not generated in this C#-only pass).
// SchemaVersion: 1

using PlantSafety.Application.SafetyCulture.CultureAssessment.LeadershipEngagementFrequency.Models;

namespace PlantSafety.Application.SafetyCulture.CultureAssessment.LeadershipEngagementFrequency.Repositories;

/// <summary>Read-only repository for LeadershipEngagementFrequency data. Implemented in the Infrastructure layer.</summary>
public interface ILeadershipEngagementFrequencyRepository
{
    /// <summary>Returns LeadershipEngagementFrequency items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<LeadershipEngagementFrequencyItemDto>> GetItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
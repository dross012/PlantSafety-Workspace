// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the Lagging Indicators (TRIR, DART, Lost Workdays) read-model.
// SchemaVersion: 1

using PlantSafety.Application.Scorecards.LaggingIndicators.Models;

namespace PlantSafety.Application.Scorecards.LaggingIndicators.Repositories;

/// <summary>Read-only repository for Lagging Indicators (TRIR, DART, Lost Workdays) data. Implemented in the Infrastructure layer.</summary>
public interface ILaggingIndicatorsRepository
{
    /// <summary>Returns Lagging Indicators (TRIR, DART, Lost Workdays) items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<LaggingIndicatorsItemDto>> GetLaggingIndicatorsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
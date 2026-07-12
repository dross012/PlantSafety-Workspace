// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the Schedules read-model.
// SchemaVersion: 1

using PlantSafety.Application.Analytics.Schedules.Models;

namespace PlantSafety.Application.Analytics.Schedules.Repositories;

/// <summary>Read-only repository for Schedules data. Implemented in the Infrastructure layer.</summary>
public interface ISchedulesRepository
{
    /// <summary>Returns Schedules items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<SchedulesItemDto>> GetScheduleItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}

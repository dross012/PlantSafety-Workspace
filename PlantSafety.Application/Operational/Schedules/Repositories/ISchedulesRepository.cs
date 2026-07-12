// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the Schedules read-model.
// SchemaVersion: 1

using PlantSafety.Application.Operational.Schedules.Models;

namespace PlantSafety.Application.Operational.Schedules.Repositories;

/// <summary>Read-only repository for Schedules data. Implemented in the Infrastructure layer.</summary>
public interface ISchedulesRepository
{
    /// <summary>Returns Schedules items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<SchedulesItemDto>> GetSchedulesAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}

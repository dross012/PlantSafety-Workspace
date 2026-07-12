// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the Webinars read-model.
// SchemaVersion: 1

using PlantSafety.Application.Operational.Training.Webinars.Models;

namespace PlantSafety.Application.Operational.Training.Webinars.Repositories;

/// <summary>Read-only repository for Webinars data. Implemented in the Infrastructure layer.</summary>
public interface IWebinarsRepository
{
    /// <summary>Returns Webinars items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<WebinarItemDto>> GetWebinarsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}

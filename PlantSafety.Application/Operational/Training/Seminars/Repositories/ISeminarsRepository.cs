// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the Seminars read-model.
// SchemaVersion: 1

using PlantSafety.Application.Operational.Training.Seminars.Models;

namespace PlantSafety.Application.Operational.Training.Seminars.Repositories;

/// <summary>Read-only repository for Seminars data. Implemented in the Infrastructure layer.</summary>
public interface ISeminarsRepository
{
    /// <summary>Returns Seminars items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<SeminarItemDto>> GetSeminarsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}

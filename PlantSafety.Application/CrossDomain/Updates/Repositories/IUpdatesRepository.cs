// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the Updates read-model.
// SchemaVersion: 1

using PlantSafety.Application.CrossDomain.Updates.Models;

namespace PlantSafety.Application.CrossDomain.Updates.Repositories;

/// <summary>Read-only repository for Updates data. Implemented in the Infrastructure layer.</summary>
public interface IUpdatesRepository
{
    /// <summary>Returns Updates items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<UpdatesItemDto>> GetUpdatesAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}

// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the Ergonomics read-model.
// SchemaVersion: 1

using PlantSafety.Application.Operational.Ergonomics.Models;

namespace PlantSafety.Application.Operational.Ergonomics.Repositories;

/// <summary>Read-only repository for Ergonomics data. Implemented in the Infrastructure layer.</summary>
public interface IErgonomicsRepository
{
    /// <summary>Returns Ergonomics items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<ErgonomicsItemDto>> GetErgonomicsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}

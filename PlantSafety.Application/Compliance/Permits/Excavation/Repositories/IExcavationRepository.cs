// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the Excavation read-model.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Permits.Excavation.Models;

namespace PlantSafety.Application.Compliance.Permits.Excavation.Repositories;

/// <summary>Read-only repository for Excavation data. Implemented in the Infrastructure layer.</summary>
public interface IExcavationRepository
{
    /// <summary>Returns Excavation items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<ExcavationItemDto>> GetExcavationAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}

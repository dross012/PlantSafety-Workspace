// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-20
// FILE DESCRIPTION: Repository abstraction for the Wellness read-model.
// SchemaVersion: 1

using PlantSafety.Application.Operational.Wellness.Models;

namespace PlantSafety.Application.Operational.Wellness.Repositories;

/// <summary>Read-only repository for Wellness data. Implemented in the Infrastructure layer.</summary>
public interface IWellnessRepository
{
    /// <summary>Returns Wellness items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<WellnessItemDto>> GetWellnessAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
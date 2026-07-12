// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the Inspections read-model.
// SchemaVersion: 1

using PlantSafety.Application.Operational.Inspections.Models;

namespace PlantSafety.Application.Operational.Inspections.Repositories;

/// <summary>Read-only repository for Inspections data. Implemented in the Infrastructure layer.</summary>
public interface IInspectionsRepository
{
    /// <summary>Returns Inspections items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<InspectionsItemDto>> GetInspectionsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}

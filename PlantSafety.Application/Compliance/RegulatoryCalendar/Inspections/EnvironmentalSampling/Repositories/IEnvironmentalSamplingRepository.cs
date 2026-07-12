// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the EnvironmentalSampling read-model.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.RegulatoryCalendar.Inspections.EnvironmentalSampling.Models;

namespace PlantSafety.Application.Compliance.RegulatoryCalendar.Inspections.EnvironmentalSampling.Repositories;

/// <summary>Read-only repository for EnvironmentalSampling data. Implemented in the Infrastructure layer.</summary>
public interface IEnvironmentalSamplingRepository
{
    /// <summary>Returns EnvironmentalSampling items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<EnvironmentalSamplingItemDto>> GetInspectionsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}

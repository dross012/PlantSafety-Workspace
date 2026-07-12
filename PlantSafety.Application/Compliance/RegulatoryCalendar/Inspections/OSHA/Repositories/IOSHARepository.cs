// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the OSHA read-model.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.RegulatoryCalendar.Inspections.OSHA.Models;

namespace PlantSafety.Application.Compliance.RegulatoryCalendar.Inspections.OSHA.Repositories;

/// <summary>Read-only repository for OSHA data. Implemented in the Infrastructure layer.</summary>
public interface IOSHARepository
{
    /// <summary>Returns OSHA items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<OSHAItemDto>> GetInspectionsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}

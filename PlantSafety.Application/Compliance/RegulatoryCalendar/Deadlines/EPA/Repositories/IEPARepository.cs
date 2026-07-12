// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the EPA read-model.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.RegulatoryCalendar.Deadlines.EPA.Models;

namespace PlantSafety.Application.Compliance.RegulatoryCalendar.Deadlines.EPA.Repositories;

/// <summary>Read-only repository for EPA data. Implemented in the Infrastructure layer.</summary>
public interface IEPARepository
{
    /// <summary>Returns EPA items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<EPAItemDto>> GetDeadlinesAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}

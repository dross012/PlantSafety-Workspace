// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the ISO read-model.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.RegulatoryCalendar.Renewals.ISO.Models;

namespace PlantSafety.Application.Compliance.RegulatoryCalendar.Renewals.ISO.Repositories;

/// <summary>Read-only repository for ISO data. Implemented in the Infrastructure layer.</summary>
public interface IISORepository
{
    /// <summary>Returns ISO items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<ISOItemDto>> GetRenewalsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}

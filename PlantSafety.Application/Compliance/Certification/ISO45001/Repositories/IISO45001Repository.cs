// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the ISO45001 read-model.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Certification.ISO45001.Models;

namespace PlantSafety.Application.Compliance.Certification.ISO45001.Repositories;

/// <summary>Read-only repository for ISO45001 data. Implemented in the Infrastructure layer.</summary>
public interface IISO45001Repository
{
    /// <summary>Returns ISO45001 items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<ISO45001ItemDto>> GetISO45001Async(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}

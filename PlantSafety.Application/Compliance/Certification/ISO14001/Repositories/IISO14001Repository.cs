// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the ISO14001 read-model.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Certification.ISO14001.Models;

namespace PlantSafety.Application.Compliance.Certification.ISO14001.Repositories;

/// <summary>Read-only repository for ISO14001 data. Implemented in the Infrastructure layer.</summary>
public interface IISO14001Repository
{
    /// <summary>Returns ISO14001 items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<ISO14001ItemDto>> GetISO14001Async(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}

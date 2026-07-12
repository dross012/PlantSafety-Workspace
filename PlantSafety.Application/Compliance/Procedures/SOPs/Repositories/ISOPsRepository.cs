// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the SOPs read-model.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Procedures.SOPs.Models;

namespace PlantSafety.Application.Compliance.Procedures.SOPs.Repositories;

/// <summary>Read-only repository for SOPs data. Implemented in the Infrastructure layer.</summary>
public interface ISOPsRepository
{
    /// <summary>Returns SOPs items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<SOPsItemDto>> GetSOPsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}

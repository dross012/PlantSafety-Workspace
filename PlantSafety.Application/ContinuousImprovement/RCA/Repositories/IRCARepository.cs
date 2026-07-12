// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the RCA read-model.
// SchemaVersion: 1

using PlantSafety.Application.ContinuousImprovement.RCA.Models;

namespace PlantSafety.Application.ContinuousImprovement.RCA.Repositories;

/// <summary>Read-only repository for RCA data. Implemented in the Infrastructure layer.</summary>
public interface IRCARepository
{
    /// <summary>Returns RCA items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<RCAItemDto>> GetRcaRecordsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}

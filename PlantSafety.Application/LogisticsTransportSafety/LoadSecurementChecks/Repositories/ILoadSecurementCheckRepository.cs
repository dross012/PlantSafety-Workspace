// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Repository abstraction for the LoadSecurementCheck read-model.
// SchemaVersion: 1

using PlantSafety.Application.LogisticsTransportSafety.LoadSecurementChecks.Models;

namespace PlantSafety.Application.LogisticsTransportSafety.LoadSecurementChecks.Repositories;

/// <summary>Read-only repository for LoadSecurementCheck data. Implemented in the Infrastructure layer.</summary>
public interface ILoadSecurementCheckRepository
{
    /// <summary>Returns LoadSecurementCheck items with a Failed status for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<LoadSecurementCheckItemDto>> ListLoadSecurementFailuresAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}

// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Repository abstraction for the Carrier read-model.
// SchemaVersion: 1

using PlantSafety.Application.LogisticsTransportSafety.Carriers.Models;

namespace PlantSafety.Application.LogisticsTransportSafety.Carriers.Repositories;

/// <summary>Read-only repository for Carrier data. Implemented in the Infrastructure layer.</summary>
public interface ICarrierRepository
{
    /// <summary>Returns Carrier items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<CarrierItemDto>> GetCarrierItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}

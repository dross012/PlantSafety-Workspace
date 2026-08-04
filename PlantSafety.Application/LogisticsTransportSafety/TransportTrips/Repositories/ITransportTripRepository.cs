// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Repository abstraction for the TransportTrip read-model.
// SchemaVersion: 1

using PlantSafety.Application.LogisticsTransportSafety.TransportTrips.Models;

namespace PlantSafety.Application.LogisticsTransportSafety.TransportTrips.Repositories;

/// <summary>Read-only repository for TransportTrip data. Implemented in the Infrastructure layer.</summary>
public interface ITransportTripRepository
{
    /// <summary>Returns active (Planned or InProgress) TransportTrip items for the tenant/site.</summary>
    Task<IReadOnlyList<TransportTripItemDto>> ListActiveTripsAsync(
        Guid tenantId,
        Guid? siteId,
        CancellationToken cancellationToken);
}

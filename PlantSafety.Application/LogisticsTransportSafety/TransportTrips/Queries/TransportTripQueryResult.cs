// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Result payload for ListActiveTripsQuery.
// SchemaVersion: 1

using PlantSafety.Application.LogisticsTransportSafety.TransportTrips.Models;

namespace PlantSafety.Application.LogisticsTransportSafety.TransportTrips.Queries;

/// <summary>Result of <see cref="ListActiveTripsQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching active TransportTrip items.</param>
public sealed record TransportTripQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<TransportTripItemDto> Items);

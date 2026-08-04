// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Query to list all active (Planned or InProgress) TransportTrip records.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.LogisticsTransportSafety.TransportTrips.Queries;

/// <summary>Retrieves all active TransportTrip records for the given tenant, optionally scoped to a site.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site filter. Null returns data for all accessible sites.</param>
public sealed record ListActiveTripsQuery(
    Guid TenantId,
    Guid? SiteId) : IRequest<Result<TransportTripQueryResult>>;

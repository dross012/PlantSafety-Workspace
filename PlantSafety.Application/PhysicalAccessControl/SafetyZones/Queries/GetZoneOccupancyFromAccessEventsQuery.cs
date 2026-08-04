// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Query to retrieve a SafetyZone's current occupancy, correlated from
//   ingested AccessEvents.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.PhysicalAccessControl.SafetyZones.Queries;

/// <summary>Retrieves the current occupancy of a single SafetyZone, correlated from ingested AccessEvents.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SafetyZoneId">Identity of the SafetyZone to retrieve occupancy for.</param>
public sealed record GetZoneOccupancyFromAccessEventsQuery(
    Guid TenantId,
    Guid SafetyZoneId) : IRequest<Result<GetZoneOccupancyFromAccessEventsResult>>;

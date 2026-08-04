// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Query to retrieve occupancy reading history for an evacuation zone within a
//   date range.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationZones.Models;

namespace PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationZones.Queries;

/// <summary>Retrieves occupancy readings for the given zone within the inclusive UTC date range.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site filter. Null returns data for all accessible sites.</param>
/// <param name="ZoneId">Identity of the evacuation zone to filter by.</param>
/// <param name="From">Inclusive start of the date range (UTC).</param>
/// <param name="To">Inclusive end of the date range (UTC).</param>
public sealed record GetOccupancyByZoneQuery(
    Guid TenantId, Guid? SiteId, Guid ZoneId, DateTime From, DateTime To) : IRequest<Result<IReadOnlyList<OccupancyReadingItemDto>>>;

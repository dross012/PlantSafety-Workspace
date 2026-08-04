// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Query to list EvacuationRoute read-model records for a given zone.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationRoutes.Models;

namespace PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationRoutes.Queries;

/// <summary>Lists EvacuationRoute records belonging to the given evacuation zone.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="ZoneId">Identity of the evacuation zone to filter by.</param>
public sealed record ListEvacuationRoutesByZoneQuery(Guid TenantId, Guid ZoneId) : IRequest<Result<IReadOnlyList<EvacuationRouteItemDto>>>;

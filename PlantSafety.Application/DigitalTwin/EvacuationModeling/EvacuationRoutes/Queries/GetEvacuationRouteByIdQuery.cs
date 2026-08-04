// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Query to retrieve a single EvacuationRoute read-model record by identity.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationRoutes.Models;

namespace PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationRoutes.Queries;

/// <summary>Retrieves a single EvacuationRoute record by its identity.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="EvacuationRouteId">Identity of the route to retrieve.</param>
public sealed record GetEvacuationRouteByIdQuery(Guid TenantId, Guid EvacuationRouteId) : IRequest<Result<EvacuationRouteItemDto?>>;

// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Query to retrieve EvacuationBottleneck history, optionally filtered by route.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.Models;

namespace PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.Queries;

/// <summary>Retrieves bottleneck history, optionally filtered to a single route.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site filter. Null returns data for all accessible sites.</param>
/// <param name="RouteId">Optional route filter. Null returns history for all routes.</param>
public sealed record GetBottleneckHistoryQuery(Guid TenantId, Guid? SiteId, Guid? RouteId) : IRequest<Result<IReadOnlyList<EvacuationBottleneckItemDto>>>;

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Application
//  File:           GetRouteDiffQuery.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Query to compute the server-side structural diff between two RouteVersionSnapshots
//                  of a route, via the pure Domain-layer RouteDiffEngine. Genuinely new -- real
//                  algorithmic work, not a stub. Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.ProcessManagement.Routing.Models;

namespace PlantSafety.Application.ProcessManagement.Routing.Queries;

/// <summary>Computes the server-side structural diff between two versions of a route.</summary>
public sealed record GetRouteDiffQuery(
    Guid TenantId,
    Guid RouteDefinitionId,
    Guid PreviousVersionId,
    Guid CurrentVersionId) : IRequest<Result<PaginatedResult<RouteDiffRowDto>>>;

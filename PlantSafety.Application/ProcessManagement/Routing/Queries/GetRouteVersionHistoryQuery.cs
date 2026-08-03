// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Application
//  File:           GetRouteVersionHistoryQuery.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve the paginated version-history timeline for a route, sourced
//                  from its persisted RouteVersionSnapshot records. Genuinely new --
//                  ProcessDefinition has no persisted version-snapshot history to query.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.ProcessManagement.Routing.Models;

namespace PlantSafety.Application.ProcessManagement.Routing.Queries;

/// <summary>Retrieves a paginated version-history timeline for a route definition.</summary>
public sealed record GetRouteVersionHistoryQuery(
    Guid TenantId,
    Guid RouteDefinitionId,
    int PageNumber,
    int PageSize) : IRequest<Result<PaginatedResult<RouteVersionSummaryDto>>>;

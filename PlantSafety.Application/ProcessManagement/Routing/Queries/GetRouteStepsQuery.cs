// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Application
//  File:           GetRouteStepsQuery.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve the ordered step list for a route, each with its
//                  domain-computed readiness status. Genuinely new -- ProcessDefinition has no
//                  structural content to query. Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.ProcessManagement.Routing.Models;

namespace PlantSafety.Application.ProcessManagement.Routing.Queries;

/// <summary>Retrieves the ordered step list for a route definition.</summary>
public sealed record GetRouteStepsQuery(
    Guid TenantId,
    Guid RouteDefinitionId) : IRequest<Result<IReadOnlyList<RouteStepItemDto>>>;

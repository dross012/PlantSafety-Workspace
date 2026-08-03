// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Application
//  File:           GetRouteDefinitionByIdQuery.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve a single RouteDefinition read-model record by identity,
//                  including its current ETag. Mirrors
//                  PlantSafety.Application.ProcessManagement.Queries.GetProcessDefinitionByIdQuery.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.ProcessManagement.Routing.Models;

namespace PlantSafety.Application.ProcessManagement.Routing.Queries;

/// <summary>Retrieves a single RouteDefinition read-model record by identity.</summary>
public sealed record GetRouteDefinitionByIdQuery(
    Guid TenantId,
    Guid RouteDefinitionId) : IRequest<Result<RouteDefinitionDetailDto>>;

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Application
//  File:           GetRouteSimulationQuery.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Query to run a server-side what-if simulation over a route's current ordered
//                  step sequence, via the pure Domain-layer RouteSimulationEngine. Genuinely new
//                  -- real critical-path/bottleneck-detection algorithmic work, not a stub. The
//                  UI's own ApiClient header explicitly requires this logic to live server-side.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.ProcessManagement.Routing.Models;

namespace PlantSafety.Application.ProcessManagement.Routing.Queries;

/// <summary>Runs a server-side simulation over a route's current ordered step sequence.</summary>
public sealed record GetRouteSimulationQuery(
    Guid TenantId,
    Guid RouteDefinitionId) : IRequest<Result<RouteSimulationResultDto>>;

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EvacuationRoutes
//  Layer:          Domain
//  File:           RouteObstructionNotFoundException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when an open RouteObstruction cannot be found on an EvacuationRoute.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.Exceptions;

/// <summary>Thrown when an open <see cref="RouteObstructionEntry"/> cannot be found on an <see cref="EvacuationRoute"/>.</summary>
public sealed class RouteObstructionNotFoundException : EvacuationRouteDomainException
{
    /// <summary>Creates a not-found exception for the given obstruction identity.</summary>
    public RouteObstructionNotFoundException(
        EvacuationRouteId routeId, RouteObstructionId obstructionId,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Open RouteObstruction '{obstructionId}' was not found on EvacuationRoute '{routeId}'.",
            "EvacuationRoute.ObstructionNotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EvacuationRoutes
//  Layer:          Domain
//  File:           EvacuationRouteNotFoundException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when an EvacuationRoute aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.Exceptions;

/// <summary>Thrown when an <see cref="EvacuationRoute"/> aggregate lookup finds no match.</summary>
public sealed class EvacuationRouteNotFoundException : EvacuationRouteDomainException
{
    /// <summary>Creates a not-found exception for the given EvacuationRoute identity.</summary>
    public EvacuationRouteNotFoundException(EvacuationRouteId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"EvacuationRoute with id '{id}' was not found.",
            "EvacuationRoute.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EvacuationRoutes
//  Layer:          Domain
//  File:           EvacuationRouteInvalidStatusTransitionException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when an EvacuationRoute lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.Exceptions;

/// <summary>Thrown when an <see cref="EvacuationRoute"/> lifecycle transition is not permitted.</summary>
public sealed class EvacuationRouteInvalidStatusTransitionException : EvacuationRouteDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public EvacuationRouteInvalidStatusTransitionException(
        EvacuationRouteId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition 'EvacuationRoute' {id} from '{from}' to '{to}'.",
            "EvacuationRoute.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}

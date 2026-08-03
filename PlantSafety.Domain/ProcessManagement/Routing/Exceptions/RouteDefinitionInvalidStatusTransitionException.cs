// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Domain
//  File:           RouteDefinitionInvalidStatusTransitionException.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when a RouteDefinition lifecycle transition is not permitted. Mirrors
//                  PlantSafety.Domain.ProcessManagement.Exceptions.ProcessDefinitionInvalidStatusTransitionException.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.ProcessManagement.Routing.ValueObjects;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Domain.ProcessManagement.Routing.Exceptions;

/// <summary>Thrown when a <c>RouteDefinition</c> lifecycle transition is not permitted.</summary>
public sealed class RouteDefinitionInvalidStatusTransitionException : RouteDefinitionDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public RouteDefinitionInvalidStatusTransitionException(
        RouteDefinitionId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition 'RouteDefinition' {id} from '{from}' to '{to}'.",
            "ProcessManagement.Routing.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}

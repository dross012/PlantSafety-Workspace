// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Domain
//  File:           RouteDefinitionValidationException.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when a RouteDefinition structural invariant is violated (e.g. an empty
//                  step list, or a Revise attempt with duplicate step numbers). Genuinely new --
//                  ProcessDefinition has no structural content to validate.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.ProcessManagement.Routing.ValueObjects;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Domain.ProcessManagement.Routing.Exceptions;

/// <summary>Thrown when a <c>RouteDefinition</c> structural invariant is violated.</summary>
public sealed class RouteDefinitionValidationException : RouteDefinitionDomainException
{
    /// <summary>Creates a structural-validation exception for the given route and reason.</summary>
    public RouteDefinitionValidationException(
        RouteDefinitionId id, string reason,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"RouteDefinition {id} failed structural validation: {reason}",
            "ProcessManagement.Routing.InvalidStructure",
            "Validation",
            "Medium",
            correlationId,
            causationId)
    {
    }
}

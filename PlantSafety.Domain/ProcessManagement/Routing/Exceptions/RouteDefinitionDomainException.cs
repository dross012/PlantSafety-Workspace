// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Domain
//  File:           RouteDefinitionDomainException.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the ProcessManagement.Routing
//                  submodule. Mirrors
//                  PlantSafety.Domain.ProcessManagement.Exceptions.ProcessDefinitionDomainException.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Common.Exceptions;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Domain.ProcessManagement.Routing.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the ProcessManagement.Routing submodule.</summary>
public abstract class RouteDefinitionDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected RouteDefinitionDomainException(
        string message,
        string errorCode,
        string errorCategory,
        string severity,
        CorrelationId correlationId,
        CausationId causationId)
        : base(message, errorCode, errorCategory, severity, correlationId, causationId)
    {
    }
}

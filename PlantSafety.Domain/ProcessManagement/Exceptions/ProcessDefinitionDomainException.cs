// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement
//  Layer:          Domain
//  File:           ProcessDefinitionDomainException.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the ProcessManagement
//                  module. Phase 4 full build-out. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ProcessManagement.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the ProcessManagement module.</summary>
public abstract class ProcessDefinitionDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected ProcessDefinitionDomainException(
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

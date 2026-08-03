// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Domain
//  File:           WorkflowDomainException.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the Workflow module.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.Domain.Common.Exceptions;

namespace PlantSafety.Domain.Workflow.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the Workflow module.</summary>
public abstract class WorkflowDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected WorkflowDomainException(
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

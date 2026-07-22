// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AccidentManagement
//  Layer:          Domain
//  File:           AccidentDomainException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the AccidentManagement module.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.Domain.Common.Exceptions;

namespace PlantSafety.Domain.AccidentManagement.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the AccidentManagement module.</summary>
public abstract class AccidentDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected AccidentDomainException(
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
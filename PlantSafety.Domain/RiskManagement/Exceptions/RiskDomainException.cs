// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         RiskManagement
//  Layer:          Domain
//  File:           RiskDomainException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the RiskManagement module.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.Domain.Common.Exceptions;

namespace PlantSafety.Domain.RiskManagement.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the RiskManagement module.</summary>
public abstract class RiskDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected RiskDomainException(
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
// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyPolicies
//  Layer:          Domain
//  File:           SafetyPolicyDomainException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the SafetyPolicies module.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.Domain.Common.Exceptions;

namespace PlantSafety.Domain.SafetyPolicies.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the SafetyPolicies module.</summary>
public abstract class SafetyPolicyDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected SafetyPolicyDomainException(
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
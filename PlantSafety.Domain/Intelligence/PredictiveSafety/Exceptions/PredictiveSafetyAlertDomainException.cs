// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.PredictiveSafety
//  Layer:          Domain
//  File:           PredictiveSafetyAlertDomainException.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code refactoring-agent)
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the
//                  Intelligence.PredictiveSafety module.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Intelligence.PredictiveSafety.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the Intelligence.PredictiveSafety module.</summary>
public abstract class PredictiveSafetyAlertDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected PredictiveSafetyAlertDomainException(
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

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.AITraining
//  Layer:          Domain
//  File:           AITrainingJobDomainException.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code refactoring-agent)
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the Intelligence.AITraining
//                  module. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Intelligence.AITraining.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the Intelligence.AITraining module.</summary>
public abstract class AITrainingJobDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected AITrainingJobDomainException(
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

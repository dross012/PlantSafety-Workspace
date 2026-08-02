// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContinuousImprovement.RCA
//  Layer:          Domain
//  File:           RCADomainException.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code refactoring-agent)
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the ContinuousImprovement RCA
//                  module. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContinuousImprovement.RCA.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the ContinuousImprovement RCA module.</summary>
public abstract class RCADomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected RCADomainException(
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

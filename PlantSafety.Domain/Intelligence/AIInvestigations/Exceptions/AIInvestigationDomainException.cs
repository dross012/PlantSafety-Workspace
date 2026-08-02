// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.AIInvestigations
//  Layer:          Domain
//  File:           AIInvestigationDomainException.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the Intelligence
//                  .AIInvestigations module. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Intelligence.AIInvestigations.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the Intelligence.AIInvestigations module.</summary>
public abstract class AIInvestigationDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected AIInvestigationDomainException(
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

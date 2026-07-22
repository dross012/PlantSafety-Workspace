// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyCulture.CultureAssessment
//  Layer:          Domain
//  File:           CultureAssessmentDomainException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the CultureAssessment module.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyCulture.CultureAssessment.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the CultureAssessment module.</summary>
public abstract class CultureAssessmentDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected CultureAssessmentDomainException(
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
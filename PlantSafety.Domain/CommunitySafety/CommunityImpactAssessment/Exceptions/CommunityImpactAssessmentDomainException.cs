// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityImpactAssessment
//  Layer:          Domain
//  File:           CommunityImpactAssessmentDomainException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the
//                  CommunityImpactAssessment module. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.CommunitySafety.CommunityImpactAssessment.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the CommunityImpactAssessment module.</summary>
public abstract class CommunityImpactAssessmentDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected CommunityImpactAssessmentDomainException(
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

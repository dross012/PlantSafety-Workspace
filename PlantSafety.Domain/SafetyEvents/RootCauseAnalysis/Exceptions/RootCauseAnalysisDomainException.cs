// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.RootCauseAnalysis
//  Layer:          Domain
//  File:           RootCauseAnalysisDomainException.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the RootCauseAnalysis
//                  module. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.RootCauseAnalysis.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the RootCauseAnalysis module.</summary>
public abstract class RootCauseAnalysisDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected RootCauseAnalysisDomainException(
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

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.IncidentTrackingPerformanceEvaluation
//  Layer:          Domain
//  File:           IncidentTrackingPerformanceEvaluationDomainException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the IncidentTrackingPerformanceEvaluation module.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.IncidentTrackingPerformanceEvaluation.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the IncidentTrackingPerformanceEvaluation module.</summary>
public abstract class IncidentTrackingPerformanceEvaluationDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected IncidentTrackingPerformanceEvaluationDomainException(
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
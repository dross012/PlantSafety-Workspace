// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.AutomatedIncidentDetection
//  Layer:          Domain
//  File:           AutomatedIncidentDetectionDomainException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the AutomatedIncidentDetection sub-module.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.AutomatedIncidentDetection.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the AutomatedIncidentDetection module.</summary>
public abstract class AutomatedIncidentDetectionDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected AutomatedIncidentDetectionDomainException(
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
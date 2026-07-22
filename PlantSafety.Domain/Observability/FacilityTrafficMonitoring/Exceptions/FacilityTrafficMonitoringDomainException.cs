// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.FacilityTrafficMonitoring
//  Layer:          Domain
//  File:           FacilityTrafficMonitoringDomainException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the FacilityTrafficMonitoring sub-module.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.FacilityTrafficMonitoring.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the FacilityTrafficMonitoring module.</summary>
public abstract class FacilityTrafficMonitoringDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected FacilityTrafficMonitoringDomainException(
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
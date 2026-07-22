// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.EnvironmentalHealthMonitoring
//  Layer:          Domain
//  File:           EnvironmentalHealthMonitoringDomainException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the EnvironmentalHealthMonitoring module.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.EnvironmentalHealthMonitoring.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the EnvironmentalHealthMonitoring module.</summary>
public abstract class EnvironmentalHealthMonitoringDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected EnvironmentalHealthMonitoringDomainException(
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
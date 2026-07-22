// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.EnvironmentalAssetMonitoring
//  Layer:          Domain
//  File:           EnvironmentalAssetMonitoringDomainException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the EnvironmentalAssetMonitoring sub-module.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.EnvironmentalAssetMonitoring.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the EnvironmentalAssetMonitoring module.</summary>
public abstract class EnvironmentalAssetMonitoringDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected EnvironmentalAssetMonitoringDomainException(
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
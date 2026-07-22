// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.ActiveMonitoringAuditing
//  Layer:          Domain
//  File:           ActiveMonitoringAuditingDomainException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the ActiveMonitoringAuditing module.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.ActiveMonitoringAuditing.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the ActiveMonitoringAuditing module.</summary>
public abstract class ActiveMonitoringAuditingDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected ActiveMonitoringAuditingDomainException(
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
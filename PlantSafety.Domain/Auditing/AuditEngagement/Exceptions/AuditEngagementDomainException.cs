// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditEngagement
//  Layer:          Domain
//  File:           AuditEngagementDomainException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the AuditEngagement module.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Auditing.AuditEngagement.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the AuditEngagement module.</summary>
public abstract class AuditEngagementDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected AuditEngagementDomainException(
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
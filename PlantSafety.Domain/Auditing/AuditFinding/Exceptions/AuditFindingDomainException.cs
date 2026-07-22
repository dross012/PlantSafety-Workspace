// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditFinding
//  Layer:          Domain
//  File:           AuditFindingDomainException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the AuditFinding module.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Auditing.AuditFinding.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the AuditFinding module.</summary>
public abstract class AuditFindingDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected AuditFindingDomainException(
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
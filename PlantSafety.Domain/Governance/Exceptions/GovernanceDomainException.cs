// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Governance
//  Layer:          Domain
//  File:           GovernanceDomainException.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the Governance module.
//                  Mirrors PlantSafety.Domain.Compliance.Auditing.Exceptions.AuditingDomainException.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Governance.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the Governance module.</summary>
public abstract class GovernanceDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected GovernanceDomainException(
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

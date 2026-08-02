// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnterpriseFabric
//  Layer:          Domain
//  File:           EnterpriseFabricDomainException.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the EnterpriseFabric module.
//                  Mirrors PlantSafety.Domain.Compliance.Auditing.Exceptions.AuditingDomainException.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnterpriseFabric.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the EnterpriseFabric module.</summary>
public abstract class EnterpriseFabricDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected EnterpriseFabricDomainException(
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

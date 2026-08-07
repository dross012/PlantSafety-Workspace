// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Packaging
//  Layer:          Domain
//  File:           PackagingSpecificationDomainException.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the PHMSA Packaging
//                  Compliance module.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.PHMSA.Packaging.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the PHMSA Packaging Compliance module.</summary>
public abstract class PackagingSpecificationDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected PackagingSpecificationDomainException(
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

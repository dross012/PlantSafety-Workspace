// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.DisposalCertificates
//  Layer:          Domain
//  File:           DisposalCertificateDomainException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Abstract base for all domain exceptions raised in the DisposalCertificates module.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HazardousWasteManagement.DisposalCertificates.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the DisposalCertificates module.</summary>
public abstract class DisposalCertificateDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected DisposalCertificateDomainException(
        string message, string errorCode, string errorCategory, string severity,
        CorrelationId correlationId, CausationId causationId)
        : base(message, errorCode, errorCategory, severity, correlationId, causationId)
    {
    }
}

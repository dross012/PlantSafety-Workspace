// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.DisposalCertificates
//  Layer:          Domain
//  File:           DisposalCertificateNotFoundException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Thrown when a DisposalCertificate aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.DisposalCertificates.ValueObjects.Ids;

namespace PlantSafety.Domain.HazardousWasteManagement.DisposalCertificates.Exceptions;

/// <summary>Thrown when a DisposalCertificate aggregate lookup finds no match.</summary>
public sealed class DisposalCertificateNotFoundException : DisposalCertificateDomainException
{
    /// <summary>Creates a not-found exception for the given DisposalCertificate identity.</summary>
    public DisposalCertificateNotFoundException(DisposalCertificateId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"DisposalCertificate with id '{id}' was not found.",
            "DisposalCertificate.NotFound", "NotFound", "Medium", correlationId, causationId)
    {
    }
}

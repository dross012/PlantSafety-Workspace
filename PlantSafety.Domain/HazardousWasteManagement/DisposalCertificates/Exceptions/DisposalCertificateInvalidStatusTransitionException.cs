// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.DisposalCertificates
//  Layer:          Domain
//  File:           DisposalCertificateInvalidStatusTransitionException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Thrown when a DisposalCertificate lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.DisposalCertificates.ValueObjects.Ids;

namespace PlantSafety.Domain.HazardousWasteManagement.DisposalCertificates.Exceptions;

/// <summary>Thrown when a DisposalCertificate lifecycle transition is not permitted.</summary>
public sealed class DisposalCertificateInvalidStatusTransitionException : DisposalCertificateDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public DisposalCertificateInvalidStatusTransitionException(
        DisposalCertificateId id, string from, string to, CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition 'DisposalCertificate' {id} from '{from}' to '{to}'.",
            "DisposalCertificate.InvalidTransition", "Lifecycle", "Medium", correlationId, causationId)
    {
    }
}

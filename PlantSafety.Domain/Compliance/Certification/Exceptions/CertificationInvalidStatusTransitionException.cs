// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Certification
//  Layer:          Domain
//  File:           CertificationInvalidStatusTransitionException.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Certification.Exceptions;

/// <summary>Thrown when a <see cref="Certification"/> lifecycle transition is not permitted.</summary>
public sealed class CertificationInvalidStatusTransitionException : CertificationDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public CertificationInvalidStatusTransitionException(
        CertificationId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            "Cannot transition 'Certification' {id} from '{from}' to '{to}'.",
            "Certification.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}

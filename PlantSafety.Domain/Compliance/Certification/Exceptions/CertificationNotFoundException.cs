// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Certification
//  Layer:          Domain
//  File:           CertificationNotFoundException.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Certification.Exceptions;

/// <summary>Thrown when a <see cref="Certification"/> aggregate lookup finds no match.</summary>
public sealed class CertificationNotFoundException : CertificationDomainException
{
    /// <summary>Creates a not-found exception for the given Certification identity.</summary>
    public CertificationNotFoundException(CertificationId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "Certification with id '{id}' was not found.",
            "Certification.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}

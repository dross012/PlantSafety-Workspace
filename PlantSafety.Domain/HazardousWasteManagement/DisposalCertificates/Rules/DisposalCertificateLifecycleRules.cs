// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.DisposalCertificates
//  Layer:          Domain
//  File:           DisposalCertificateLifecycleRules.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Defines permitted lifecycle transitions for the DisposalCertificate aggregate.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.DisposalCertificates.Enums;

namespace PlantSafety.Domain.HazardousWasteManagement.DisposalCertificates.Rules;

/// <summary>Defines permitted lifecycle transitions for the DisposalCertificate aggregate.</summary>
public static class DisposalCertificateLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the certificate may be verified from <paramref name="current"/>.</summary>
    public static bool CanVerify(DisposalCertificateStatus current) => current == DisposalCertificateStatus.Received;
}

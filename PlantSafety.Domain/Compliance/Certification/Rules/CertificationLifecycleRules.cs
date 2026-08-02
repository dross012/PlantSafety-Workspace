// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Certification
//  Layer:          Domain
//  File:           CertificationLifecycleRules.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Certification.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="Certification"/> aggregate.</summary>
public static class CertificationLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the aggregate may be activated from <paramref name="current"/>.</summary>
    public static bool CanActivate(CertificationStatus current) => current == CertificationStatus.Created;

    /// <summary>Returns <see langword="true"/> when the aggregate may be completed from <paramref name="current"/>.</summary>
    public static bool CanComplete(CertificationStatus current) => current == CertificationStatus.Active;

    /// <summary>Returns <see langword="true"/> when the aggregate may be renewed from <paramref name="current"/>.</summary>
    public static bool CanRenew(CertificationStatus current) => current == CertificationStatus.Active;

    /// <summary>Returns <see langword="true"/> when the aggregate may be closed from <paramref name="current"/>.</summary>
    public static bool CanClose(CertificationStatus current) => current == CertificationStatus.Completed;

    /// <summary>Returns <see langword="true"/> when the aggregate may be archived from <paramref name="current"/>.</summary>
    public static bool CanArchive(CertificationStatus current) => current == CertificationStatus.Closed;
}

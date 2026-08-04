// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.DisposalCertificates
//  Layer:          Domain
//  File:           DisposalCertificateStatus.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Lifecycle states for the DisposalCertificate aggregate root (certificate of
//                  destruction/disposal received from a TSDF facility).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HazardousWasteManagement.DisposalCertificates.Enums;

/// <summary>Lifecycle states for the DisposalCertificate aggregate root.</summary>
public enum DisposalCertificateStatus
{
    /// <summary>Certificate received and recorded, pending verification.</summary>
    Received = 0,

    /// <summary>Certificate reviewed and verified against the manifest.</summary>
    Verified = 1,
}

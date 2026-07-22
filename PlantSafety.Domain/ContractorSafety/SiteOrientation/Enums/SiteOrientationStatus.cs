// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.SiteOrientation
//  Layer:          Domain
//  File:           SiteOrientationStatus.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Real orientation lifecycle states for the SiteOrientation aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.SiteOrientation.Enums;

/// <summary>Lifecycle states for the <see cref="SiteOrientation"/> aggregate root.</summary>
public enum SiteOrientationStatus
{
    /// <summary>Orientation session scheduled for a contractor.</summary>
    Scheduled = 0,

    /// <summary>Orientation delivered to contractor personnel.</summary>
    Delivered = 1,

    /// <summary>Comprehension verified; contractor cleared for site access.</summary>
    ComprehensionVerified = 2,

    /// <summary>Orientation has expired; re-orientation required before further site access.</summary>
    Expired = 3,
}

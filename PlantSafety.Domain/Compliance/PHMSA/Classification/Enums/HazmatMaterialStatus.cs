// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Classification
//  Layer:          Domain
//  File:           HazmatMaterialStatus.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the HazmatMaterial aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.PHMSA.Classification.Enums;

/// <summary>Lifecycle states for the <c>HazmatMaterial</c> aggregate root.</summary>
public enum HazmatMaterialStatus
{
    /// <summary>Material record has been started but not yet classified.</summary>
    Draft = 0,

    /// <summary>Material has been classified under 49 CFR and is the active record.</summary>
    Classified = 1,

    /// <summary>Classification has been superseded by a newer classification record.</summary>
    Superseded = 2,

    /// <summary>Classification record has been retired and is no longer authoritative.</summary>
    Retired = 3,
}

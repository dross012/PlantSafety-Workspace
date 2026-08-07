// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Placarding
//  Layer:          Domain
//  File:           HazmatContainerStatus.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the HazmatContainer aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.PHMSA.Placarding.Enums;

/// <summary>Lifecycle states for the HazmatContainer aggregate root.</summary>
public enum HazmatContainerStatus
{
    /// <summary>Container record created; markings/labels not yet applied.</summary>
    Draft = 0,

    /// <summary>Text markings and hazard labels have been applied.</summary>
    MarkedAndLabeled = 1,

    /// <summary>Placarding requirements have been computed and recorded.</summary>
    PlacardingCalculated = 2,

    /// <summary>Container record retired; no longer in active use.</summary>
    Retired = 3,
}

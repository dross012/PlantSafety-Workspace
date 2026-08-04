// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.WasteStreams
//  Layer:          Domain
//  File:           WasteHazardClass.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    RCRA-style hazard classification for a characterized WasteStream.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HazardousWasteManagement.WasteStreams.Enums;

/// <summary>RCRA-style hazard classification for a characterized WasteStream.</summary>
public enum WasteHazardClass
{
    /// <summary>Ignitable waste (D001).</summary>
    Ignitable = 0,

    /// <summary>Corrosive waste (D002).</summary>
    Corrosive = 1,

    /// <summary>Reactive waste (D003).</summary>
    Reactive = 2,

    /// <summary>Toxic waste (D004-D043).</summary>
    Toxic = 3,

    /// <summary>Universal waste (batteries, lamps, pesticides).</summary>
    Universal = 4,

    /// <summary>Non-hazardous waste, tracked for completeness.</summary>
    NonHazardous = 5,
}

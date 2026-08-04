// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.WasteStreams
//  Layer:          Domain
//  File:           WasteStreamStatus.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Lifecycle states for the WasteStream aggregate root (a characterized source
//                  of hazardous waste generation, RCRA-style).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HazardousWasteManagement.WasteStreams.Enums;

/// <summary>Lifecycle states for the WasteStream aggregate root.</summary>
public enum WasteStreamStatus
{
    /// <summary>Waste stream drafted but not yet characterized.</summary>
    Draft = 0,

    /// <summary>Waste stream characterized (EPA waste code and hazard class assigned).</summary>
    Characterized = 1,

    /// <summary>Waste stream deactivated; no longer generating waste.</summary>
    Deactivated = 2,
}

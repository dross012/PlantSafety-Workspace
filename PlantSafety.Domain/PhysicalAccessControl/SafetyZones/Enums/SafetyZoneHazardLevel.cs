// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.SafetyZones
//  Layer:          Domain
//  File:           SafetyZoneHazardLevel.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Hazard classification for a SafetyZone, used to drive anomaly-severity
//                  scoring and RiskManagement correlation.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.SafetyZones.Enums;

/// <summary>Hazard classification for a <see cref="SafetyZone"/>.</summary>
public enum SafetyZoneHazardLevel
{
    /// <summary>Low-hazard area (offices, common areas).</summary>
    Low = 0,

    /// <summary>Medium-hazard area (general production floor).</summary>
    Medium = 1,

    /// <summary>High-hazard area (confined space, elevated work, energized equipment).</summary>
    High = 2,

    /// <summary>Critical-hazard area (permit-required confined space, high-voltage, process-safety-critical).</summary>
    Critical = 3,
}

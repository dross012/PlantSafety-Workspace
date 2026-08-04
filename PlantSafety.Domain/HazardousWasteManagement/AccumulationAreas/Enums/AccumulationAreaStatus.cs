// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.AccumulationAreas
//  Layer:          Domain
//  File:           AccumulationAreaStatus.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Lifecycle states for the AccumulationArea aggregate root (a designated
//                  satellite or central hazardous-waste accumulation area at a site).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HazardousWasteManagement.AccumulationAreas.Enums;

/// <summary>Lifecycle states for the AccumulationArea aggregate root.</summary>
public enum AccumulationAreaStatus
{
    /// <summary>Area active and available for container accumulation.</summary>
    Active = 0,

    /// <summary>Area flagged because one or more containers are nearing their regulatory time limit.</summary>
    TimeLimitWarning = 1,

    /// <summary>Area inactive; no longer in use.</summary>
    Inactive = 2,
}

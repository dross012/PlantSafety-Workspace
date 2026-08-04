// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.WasteContainers
//  Layer:          Domain
//  File:           WasteContainerStatus.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Lifecycle states for the WasteContainer aggregate root (a physical accumulation
//                  container within an AccumulationArea, subject to RCRA 90/180-day time limits).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HazardousWasteManagement.WasteContainers.Enums;

/// <summary>Lifecycle states for the WasteContainer aggregate root.</summary>
public enum WasteContainerStatus
{
    /// <summary>Container opened and accumulating waste.</summary>
    Open = 0,

    /// <summary>Container closed; full or accumulation period ended.</summary>
    Closed = 1,

    /// <summary>Container shipped off-site via manifest.</summary>
    Shipped = 2,
}

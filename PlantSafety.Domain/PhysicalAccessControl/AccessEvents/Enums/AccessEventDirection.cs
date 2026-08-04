// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.AccessEvents
//  Layer:          Domain
//  File:           AccessEventDirection.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Direction of travel for an AccessEvent, used for zone occupancy correlation.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.AccessEvents.Enums;

/// <summary>Direction of travel for an <see cref="AccessEvent"/>, used for zone occupancy correlation.</summary>
public enum AccessEventDirection
{
    /// <summary>Entry into the guarded zone.</summary>
    In = 0,

    /// <summary>Exit from the guarded zone.</summary>
    Out = 1,
}

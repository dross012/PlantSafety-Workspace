// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.MachineGuardsBarriers
//  Layer:          Domain
//  File:           MachineGuardBarrierType.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Classification of machine guard/barrier types per the PlantSafety Program Structure
//                  Machine Guards & Barriers component.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.MachineGuardsBarriers.Enums;

/// <summary>Classification of machine guard/barrier types.</summary>
public enum MachineGuardBarrierType
{
    /// <summary>Fixed guard permanently blocking access to dangerous moving parts.</summary>
    FixedGuard = 0,

    /// <summary>Interlocked door that automatically stops machine operation when opened.</summary>
    InterlockedDoor = 1,

    /// <summary>Physical enclosure around robotic cells, cutting tools, or high-speed equipment.</summary>
    PhysicalEnclosure = 2,

    /// <summary>Adjustable guard for tools requiring operator positioning.</summary>
    AdjustableGuard = 3,
}

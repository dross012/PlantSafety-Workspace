// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.SafetyValvesRelays
//  Layer:          Domain
//  File:           SafetyComponentType.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Classification of safety valve/relay component types per the PlantSafety Program
//                  Structure Safety Valves & Relays component.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.SafetyValvesRelays.Enums;

/// <summary>Classification of safety valve/relay component types.</summary>
public enum SafetyComponentType
{
    /// <summary>Emergency shut-off valve for hydraulic or pneumatic systems.</summary>
    EmergencyShutOffValve = 0,

    /// <summary>Dual-channel safety relay monitoring critical circuits.</summary>
    DualChannelSafetyRelay = 1,

    /// <summary>Redundant control system preventing unsafe operation on component failure.</summary>
    RedundantControlSystem = 2,
}

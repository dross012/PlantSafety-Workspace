// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.Telemetry
//  Layer:          Domain
//  File:           MachineOperatingState.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Phase 5 Production Tracking Intelligence)
//  SchemaVersion:  2026.1
//  Description:    Real-time operating state of a monitored machine/asset, captured by
//                  telemetry ingestion for OEE availability calculations. Additive-only
//                  evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.Telemetry.Enums;

/// <summary>Real-time operating state of a monitored machine/asset.</summary>
public enum MachineOperatingState
{
    /// <summary>Machine is actively producing.</summary>
    Running = 0,

    /// <summary>Machine is powered and available but not currently producing.</summary>
    Idle = 1,

    /// <summary>Machine is undergoing changeover/setup between production runs.</summary>
    Setup = 2,

    /// <summary>Machine is stopped due to an unplanned fault/breakdown.</summary>
    Down = 3,

    /// <summary>Machine is starved or blocked by an upstream/downstream process constraint.</summary>
    Blocked = 4,
}

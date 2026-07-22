// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.TwoHandControls
//  Layer:          Domain
//  File:           TwoHandControlsStatus.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Real test lifecycle states for the TwoHandControls aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.TwoHandControls.Enums;

/// <summary>Lifecycle states for the <see cref="TwoHandControls"/> aggregate root.</summary>
public enum TwoHandControlsStatus
{
    /// <summary>Control installed and awaiting first test.</summary>
    Installed = 0,

    /// <summary>Synchronization test scheduled.</summary>
    TestScheduled = 1,

    /// <summary>Control tested and confirmed properly synchronized.</summary>
    Synchronized = 2,

    /// <summary>A bypass attempt or synchronization failure was detected.</summary>
    BypassDetected = 3,

    /// <summary>Control permanently retired/decommissioned.</summary>
    Retired = 4,
}

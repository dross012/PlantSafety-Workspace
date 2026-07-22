// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.SafetyValvesRelays
//  Layer:          Domain
//  File:           SafetyValvesRelaysStatus.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Real inspection/reliability lifecycle states for the SafetyValvesRelays aggregate
//                  root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.SafetyValvesRelays.Enums;

/// <summary>Lifecycle states for the <see cref="SafetyValvesRelays"/> aggregate root.</summary>
public enum SafetyValvesRelaysStatus
{
    /// <summary>Component installed and awaiting first inspection.</summary>
    Installed = 0,

    /// <summary>Inspection scheduled and pending.</summary>
    InspectionScheduled = 1,

    /// <summary>Component inspected and confirmed reliable.</summary>
    Reliable = 2,

    /// <summary>A fault was detected during inspection or operation.</summary>
    FaultDetected = 3,

    /// <summary>Repair is underway to remediate a detected fault.</summary>
    RepairInProgress = 4,

    /// <summary>Component permanently retired/decommissioned.</summary>
    Retired = 5,
}

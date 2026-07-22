// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.EmergencyStopDevices
//  Layer:          Domain
//  File:           EmergencyStopDevicesStatus.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Real test/reliability lifecycle states for the EmergencyStopDevices aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.EmergencyStopDevices.Enums;

/// <summary>Lifecycle states for the <see cref="EmergencyStopDevices"/> aggregate root.</summary>
public enum EmergencyStopDevicesStatus
{
    /// <summary>Device installed and awaiting first functional test.</summary>
    Installed = 0,

    /// <summary>A functional test has been scheduled.</summary>
    TestScheduled = 1,

    /// <summary>Device tested and confirmed operational.</summary>
    Operational = 2,

    /// <summary>A test found the device failed to respond reliably.</summary>
    FailureDetected = 3,

    /// <summary>Repair is underway.</summary>
    RepairInProgress = 4,

    /// <summary>Device permanently retired/decommissioned.</summary>
    Retired = 5,
}

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.PresenceSensingDevices
//  Layer:          Domain
//  File:           PresenceSensingDevicesStatus.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Real calibration lifecycle states for the PresenceSensingDevices aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.PresenceSensingDevices.Enums;

/// <summary>Lifecycle states for the <see cref="PresenceSensingDevices"/> aggregate root.</summary>
public enum PresenceSensingDevicesStatus
{
    /// <summary>Device installed and awaiting first calibration.</summary>
    Installed = 0,

    /// <summary>Calibration scheduled and pending.</summary>
    CalibrationScheduled = 1,

    /// <summary>Device calibrated and confirmed accurate.</summary>
    Calibrated = 2,

    /// <summary>Calibration found detection accuracy out of tolerance.</summary>
    OutOfTolerance = 3,

    /// <summary>Device permanently retired/decommissioned.</summary>
    Retired = 4,
}

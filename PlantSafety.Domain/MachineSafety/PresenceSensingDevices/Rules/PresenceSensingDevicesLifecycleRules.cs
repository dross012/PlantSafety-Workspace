// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.PresenceSensingDevices
//  Layer:          Domain
//  File:           PresenceSensingDevicesLifecycleRules.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Defines permitted lifecycle transitions for the PresenceSensingDevices aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.PresenceSensingDevices.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="PresenceSensingDevices"/> aggregate.</summary>
public static class PresenceSensingDevicesLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when calibration may be scheduled from <paramref name="current"/>.</summary>
    public static bool CanScheduleCalibration(PresenceSensingDevicesStatus current) =>
        current is PresenceSensingDevicesStatus.Installed or PresenceSensingDevicesStatus.Calibrated or PresenceSensingDevicesStatus.OutOfTolerance;

    /// <summary>Returns <see langword="true"/> when a calibration result may be recorded from <paramref name="current"/>.</summary>
    public static bool CanRecordCalibrationResult(PresenceSensingDevicesStatus current) => current == PresenceSensingDevicesStatus.CalibrationScheduled;

    /// <summary>Returns <see langword="true"/> when the device may be retired from <paramref name="current"/>.</summary>
    public static bool CanRetire(PresenceSensingDevicesStatus current) => current != PresenceSensingDevicesStatus.Retired;
}

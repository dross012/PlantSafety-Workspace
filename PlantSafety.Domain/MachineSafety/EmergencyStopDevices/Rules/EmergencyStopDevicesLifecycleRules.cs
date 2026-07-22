// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.EmergencyStopDevices
//  Layer:          Domain
//  File:           EmergencyStopDevicesLifecycleRules.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Defines permitted lifecycle transitions for the EmergencyStopDevices aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.EmergencyStopDevices.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="EmergencyStopDevices"/> aggregate.</summary>
public static class EmergencyStopDevicesLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when a test may be scheduled from <paramref name="current"/>.</summary>
    public static bool CanScheduleTest(EmergencyStopDevicesStatus current) =>
        current is EmergencyStopDevicesStatus.Installed or EmergencyStopDevicesStatus.Operational;

    /// <summary>Returns <see langword="true"/> when a test result may be recorded from <paramref name="current"/>.</summary>
    public static bool CanRecordTestResult(EmergencyStopDevicesStatus current) => current == EmergencyStopDevicesStatus.TestScheduled;

    /// <summary>Returns <see langword="true"/> when repair may begin from <paramref name="current"/>.</summary>
    public static bool CanBeginRepair(EmergencyStopDevicesStatus current) => current == EmergencyStopDevicesStatus.FailureDetected;

    /// <summary>Returns <see langword="true"/> when the device may be retired from <paramref name="current"/>.</summary>
    public static bool CanRetire(EmergencyStopDevicesStatus current) => current != EmergencyStopDevicesStatus.Retired;
}

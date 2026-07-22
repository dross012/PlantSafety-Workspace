// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.SafetyValvesRelays
//  Layer:          Domain
//  File:           SafetyValvesRelaysLifecycleRules.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Defines permitted lifecycle transitions for the SafetyValvesRelays aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.SafetyValvesRelays.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="SafetyValvesRelays"/> aggregate.</summary>
public static class SafetyValvesRelaysLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when an inspection may be scheduled from <paramref name="current"/>.</summary>
    public static bool CanScheduleInspection(SafetyValvesRelaysStatus current) =>
        current is SafetyValvesRelaysStatus.Installed or SafetyValvesRelaysStatus.Reliable;

    /// <summary>Returns <see langword="true"/> when an inspection result may be recorded from <paramref name="current"/>.</summary>
    public static bool CanRecordInspectionResult(SafetyValvesRelaysStatus current) => current == SafetyValvesRelaysStatus.InspectionScheduled;

    /// <summary>Returns <see langword="true"/> when repair may begin from <paramref name="current"/>.</summary>
    public static bool CanBeginRepair(SafetyValvesRelaysStatus current) => current == SafetyValvesRelaysStatus.FaultDetected;

    /// <summary>Returns <see langword="true"/> when the component may be retired from <paramref name="current"/>.</summary>
    public static bool CanRetire(SafetyValvesRelaysStatus current) => current != SafetyValvesRelaysStatus.Retired;
}

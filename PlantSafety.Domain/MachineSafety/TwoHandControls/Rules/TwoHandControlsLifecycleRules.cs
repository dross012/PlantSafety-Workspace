// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.TwoHandControls
//  Layer:          Domain
//  File:           TwoHandControlsLifecycleRules.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Defines permitted lifecycle transitions for the TwoHandControls aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.TwoHandControls.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="TwoHandControls"/> aggregate.</summary>
public static class TwoHandControlsLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when a test may be scheduled from <paramref name="current"/>.</summary>
    public static bool CanScheduleTest(TwoHandControlsStatus current) =>
        current is TwoHandControlsStatus.Installed or TwoHandControlsStatus.Synchronized;

    /// <summary>Returns <see langword="true"/> when a test result may be recorded from <paramref name="current"/>.</summary>
    public static bool CanRecordTestResult(TwoHandControlsStatus current) => current == TwoHandControlsStatus.TestScheduled;

    /// <summary>Returns <see langword="true"/> when the control may be retired from <paramref name="current"/>.</summary>
    public static bool CanRetire(TwoHandControlsStatus current) => current != TwoHandControlsStatus.Retired;
}

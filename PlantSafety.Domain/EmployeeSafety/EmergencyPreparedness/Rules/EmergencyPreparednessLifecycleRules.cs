// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.EmergencyPreparedness
//  Layer:          Domain
//  File:           EmergencyPreparednessLifecycleRules.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Defines permitted lifecycle transitions for the EmergencyPreparedness aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.EmergencyPreparedness.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="EmergencyPreparedness"/> aggregate.</summary>
public static class EmergencyPreparednessLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when a drill may be scheduled from <paramref name="current"/>.</summary>
    public static bool CanScheduleDrill(EmergencyPreparednessStatus current) =>
        current is EmergencyPreparednessStatus.Documented or EmergencyPreparednessStatus.ReadinessVerified or EmergencyPreparednessStatus.ReadinessGapIdentified;

    /// <summary>Returns <see langword="true"/> when a drill may be conducted from <paramref name="current"/>.</summary>
    public static bool CanConductDrill(EmergencyPreparednessStatus current) => current == EmergencyPreparednessStatus.DrillScheduled;

    /// <summary>Returns <see langword="true"/> when readiness may be recorded from <paramref name="current"/>.</summary>
    public static bool CanRecordReadiness(EmergencyPreparednessStatus current) => current == EmergencyPreparednessStatus.DrillConducted;
}

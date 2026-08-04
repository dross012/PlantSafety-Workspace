// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.CraneHoistSystems
//  Layer:          Domain
//  File:           CraneHoistSystemsLifecycleRules.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Defines permitted lifecycle transitions for the CraneHoistSystems aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.CraneHoistSystems.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="CraneHoistSystems"/> aggregate.</summary>
public static class CraneHoistSystemsLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when a crane inspection may be scheduled from <paramref name="current"/>.</summary>
    public static bool CanScheduleInspection(CraneHoistSystemsStatus current) =>
        current is CraneHoistSystemsStatus.Registered or CraneHoistSystemsStatus.InService or CraneHoistSystemsStatus.InspectionOverdue;

    /// <summary>Returns <see langword="true"/> when a crane inspection may be completed from <paramref name="current"/>.</summary>
    public static bool CanCompleteInspection(CraneHoistSystemsStatus current) =>
        current is CraneHoistSystemsStatus.InspectionDue or CraneHoistSystemsStatus.InspectionOverdue;

    /// <summary>Returns <see langword="true"/> when the asset may be retired from <paramref name="current"/>.</summary>
    public static bool CanRetire(CraneHoistSystemsStatus current) => current != CraneHoistSystemsStatus.Retired;
}

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.Ppe
//  Layer:          Domain
//  File:           PpeLifecycleRules.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Defines permitted lifecycle transitions for the Ppe aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.Ppe.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="Ppe"/> aggregate.</summary>
public static class PpeLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when a fit test may be recorded from <paramref name="current"/>.</summary>
    public static bool CanRecordFitTest(PpeStatus current) => current == PpeStatus.PendingFitTest;

    /// <summary>Returns <see langword="true"/> when an inspection may be recorded from <paramref name="current"/>.</summary>
    public static bool CanRecordInspection(PpeStatus current) => current == PpeStatus.InService;

    /// <summary>Returns <see langword="true"/> when the item may be flagged for replacement from <paramref name="current"/>.</summary>
    public static bool CanFlagForReplacement(PpeStatus current) => current == PpeStatus.InService;

    /// <summary>Returns <see langword="true"/> when the item may be retired from <paramref name="current"/>.</summary>
    public static bool CanRetire(PpeStatus current) => current != PpeStatus.Retired;
}

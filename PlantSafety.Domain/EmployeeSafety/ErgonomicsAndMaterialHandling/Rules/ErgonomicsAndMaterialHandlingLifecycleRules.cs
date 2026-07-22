// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.ErgonomicsAndMaterialHandling
//  Layer:          Domain
//  File:           ErgonomicsAndMaterialHandlingLifecycleRules.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Defines permitted lifecycle transitions for the ErgonomicsAndMaterialHandling aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.ErgonomicsAndMaterialHandling.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="ErgonomicsAndMaterialHandling"/> aggregate.</summary>
public static class ErgonomicsAndMaterialHandlingLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when an assessment may be recorded from <paramref name="current"/>.</summary>
    public static bool CanRecordAssessment(ErgonomicsAndMaterialHandlingStatus current) =>
        current is ErgonomicsAndMaterialHandlingStatus.AssessmentScheduled or ErgonomicsAndMaterialHandlingStatus.FollowUpScheduled;

    /// <summary>Returns <see langword="true"/> when controls may be implemented from <paramref name="current"/>.</summary>
    public static bool CanImplementControls(ErgonomicsAndMaterialHandlingStatus current) => current == ErgonomicsAndMaterialHandlingStatus.Assessed;

    /// <summary>Returns <see langword="true"/> when a follow-up may be scheduled from <paramref name="current"/>.</summary>
    public static bool CanScheduleFollowUp(ErgonomicsAndMaterialHandlingStatus current) => current == ErgonomicsAndMaterialHandlingStatus.ControlsImplemented;

    /// <summary>Returns <see langword="true"/> when the assessment may be closed from <paramref name="current"/>.</summary>
    public static bool CanClose(ErgonomicsAndMaterialHandlingStatus current) =>
        current is ErgonomicsAndMaterialHandlingStatus.ControlsImplemented or ErgonomicsAndMaterialHandlingStatus.FollowUpScheduled;
}

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.DrillsAndExercises
//  Layer:          Domain
//  File:           DrillsAndExercisesLifecycleRules.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Guards for the DrillsAndExercises aggregate's lifecycle transitions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.DrillsAndExercises.Rules;

/// <summary>Defines permitted lifecycle transitions for the DrillsAndExercises aggregate.</summary>
public static class DrillsAndExercisesLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the drill may be conducted from <paramref name="current"/>.</summary>
    public static bool CanConduct(DrillsAndExercisesStatus current) => current == DrillsAndExercisesStatus.Scheduled;

    /// <summary>Returns <see langword="true"/> when the drill may be completed from <paramref name="current"/>.</summary>
    public static bool CanComplete(DrillsAndExercisesStatus current) => current == DrillsAndExercisesStatus.Conducted;

    /// <summary>Returns <see langword="true"/> when the drill may be cancelled from <paramref name="current"/>.</summary>
    public static bool CanCancel(DrillsAndExercisesStatus current) => current == DrillsAndExercisesStatus.Scheduled;

    /// <summary>Returns <see langword="true"/> when the drill may be closed from <paramref name="current"/>.</summary>
    public static bool CanClose(DrillsAndExercisesStatus current) => current == DrillsAndExercisesStatus.Completed;

    /// <summary>Returns <see langword="true"/> when the drill may be archived from <paramref name="current"/>.</summary>
    public static bool CanArchive(DrillsAndExercisesStatus current) => current == DrillsAndExercisesStatus.Closed;
}

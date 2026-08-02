// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.DrillsAndExercises
//  Layer:          Domain
//  File:           DrillsAndExercisesStatus.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the DrillsAndExercises aggregate root. Matches the
//                  pre-existing DrillsAndExercisesItemDto's documented Scheduled/Conducted/
//                  Cancelled vocabulary plus the standard Completed/Closed/Archived closeout
//                  states used throughout this codebase.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.DrillsAndExercises.Enums;

/// <summary>Lifecycle states for the DrillsAndExercises aggregate root.</summary>
public enum DrillsAndExercisesStatus
{
    /// <summary>The drill/exercise has been scheduled and has not yet been conducted.</summary>
    Scheduled = 0,

    /// <summary>The drill/exercise has been conducted.</summary>
    Conducted = 1,

    /// <summary>The drill/exercise has been completed (post-conduct debrief recorded).</summary>
    Completed = 2,

    /// <summary>The drill/exercise record has been formally closed.</summary>
    Closed = 3,

    /// <summary>The drill/exercise record has been archived.</summary>
    Archived = 4,

    /// <summary>The scheduled drill/exercise was cancelled before being conducted.</summary>
    Cancelled = 5,
}

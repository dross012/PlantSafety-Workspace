// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.DrillsAndExercises
//  Layer:          Domain
//  File:           DrillsAndExercisesNotFoundException.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Thrown when a DrillsAndExercises aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.DrillsAndExercises.Exceptions;

/// <summary>Thrown when a DrillsAndExercises aggregate lookup finds no match.</summary>
public sealed class DrillsAndExercisesNotFoundException : DrillsAndExercisesDomainException
{
    /// <summary>Creates a not-found exception for the given DrillsAndExercises identity.</summary>
    public DrillsAndExercisesNotFoundException(DrillsAndExercisesId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"DrillsAndExercises with id '{id}' was not found.",
            "DrillsAndExercises.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}

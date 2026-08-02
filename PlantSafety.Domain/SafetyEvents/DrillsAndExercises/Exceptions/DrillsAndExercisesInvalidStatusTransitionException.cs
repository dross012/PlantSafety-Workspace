// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.DrillsAndExercises
//  Layer:          Domain
//  File:           DrillsAndExercisesInvalidStatusTransitionException.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Thrown when a DrillsAndExercises lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.DrillsAndExercises.Exceptions;

/// <summary>Thrown when a DrillsAndExercises lifecycle transition is not permitted.</summary>
public sealed class DrillsAndExercisesInvalidStatusTransitionException : DrillsAndExercisesDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public DrillsAndExercisesInvalidStatusTransitionException(
        DrillsAndExercisesId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition DrillsAndExercises '{id}' from '{from}' to '{to}'.",
            "DrillsAndExercises.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}

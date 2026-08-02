// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.DrillsAndExercises
//  Layer:          Domain
//  File:           DrillsAndExercisesEventId.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for DrillsAndExercises domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.DrillsAndExercises.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for DrillsAndExercises domain events.</summary>
public readonly record struct DrillsAndExercisesEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a DrillsAndExercisesEventId wrapping a non-empty GUID.</summary>
    public DrillsAndExercisesEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("DrillsAndExercisesEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new DrillsAndExercisesEventId using Uuid7.</summary>
    public static DrillsAndExercisesEventId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a DrillsAndExercisesEventId.</summary>
    public static DrillsAndExercisesEventId FromGuid(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}

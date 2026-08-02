// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.DrillsAndExercises
//  Layer:          Domain
//  File:           DrillsAndExercisesId.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the DrillsAndExercises aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.DrillsAndExercises.ValueObjects.Ids;

/// <summary>Strong-typed identity for the DrillsAndExercises aggregate root.</summary>
public readonly record struct DrillsAndExercisesId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a DrillsAndExercisesId wrapping a non-empty GUID.</summary>
    public DrillsAndExercisesId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("DrillsAndExercisesId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new DrillsAndExercisesId using Uuid7.</summary>
    public static DrillsAndExercisesId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a DrillsAndExercisesId.</summary>
    public static DrillsAndExercisesId FromGuid(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}

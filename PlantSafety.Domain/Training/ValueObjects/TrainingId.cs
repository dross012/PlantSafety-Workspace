// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Training
//  Layer:          Domain
//  File:           TrainingId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Strong-typed Uuid7-backed identity for the Training aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Training.ValueObjects;

/// <summary>Strong-typed, Uuid7-backed identity for the Training aggregate root.</summary>
public readonly record struct TrainingId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a TrainingId wrapping a non-empty GUID.</summary>
    public TrainingId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("TrainingId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new TrainingId using Uuid7.</summary>
    public static TrainingId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a TrainingId.</summary>
    public static TrainingId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a TrainingId.</summary>
    public static TrainingId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}

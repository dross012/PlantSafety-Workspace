// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.AITraining
//  Layer:          Domain
//  File:           AITrainingJobEventId.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code refactoring-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for Intelligence.AITraining domain events.
//                  Mirrors PlantSafety.Domain.ContinuousImprovement.RCA.ValueObjects.Ids
//                  .RCAEventId's identical precedent.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Intelligence.AITraining.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for Intelligence.AITraining domain events.</summary>
public readonly record struct AITrainingJobEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates an AITrainingJobEventId wrapping a non-empty GUID.</summary>
    public AITrainingJobEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AITrainingJobEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new AITrainingJobEventId using Uuid7.</summary>
    public static AITrainingJobEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.AITraining
//  Layer:          Domain
//  File:           AITrainingJobId.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code refactoring-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the Intelligence.AITraining
//                  AITrainingJob aggregate root. Mirrors
//                  PlantSafety.Domain.ContinuousImprovement.RCA.ValueObjects.Ids.RCAId's
//                  identical precedent.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Intelligence.AITraining.ValueObjects.Ids;

/// <summary>Strong-typed identity for the Intelligence.AITraining AITrainingJob aggregate root.</summary>
public readonly record struct AITrainingJobId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates an AITrainingJobId wrapping a non-empty GUID.</summary>
    public AITrainingJobId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AITrainingJobId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new AITrainingJobId using Uuid7.</summary>
    public static AITrainingJobId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as an AITrainingJobId.</summary>
    public static AITrainingJobId FromGuid(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}

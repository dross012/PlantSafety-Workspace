// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyPrograms.SafetyProgram
//  Layer:          Domain
//  File:           SafetyProgramGoalId.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the SafetyProgramGoal child entity.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;

namespace PlantSafety.Domain.SafetyPrograms.SafetyProgram.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the SafetyProgramGoal child entity.</summary>
public readonly record struct SafetyProgramGoalId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a SafetyProgramGoalId wrapping a non-empty GUID.</summary>
    public SafetyProgramGoalId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SafetyProgramGoalId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new SafetyProgramGoalId using Uuid7.</summary>
    public static SafetyProgramGoalId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a SafetyProgramGoalId.</summary>
    public static SafetyProgramGoalId FromGuid(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}

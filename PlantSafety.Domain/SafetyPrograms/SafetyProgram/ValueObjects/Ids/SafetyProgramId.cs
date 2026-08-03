// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyPrograms.SafetyProgram
//  Layer:          Domain
//  File:           SafetyProgramId.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the SafetyProgram aggregate root.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;

namespace PlantSafety.Domain.SafetyPrograms.SafetyProgram.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the SafetyProgram aggregate root.</summary>
public readonly record struct SafetyProgramId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a SafetyProgramId wrapping a non-empty GUID.</summary>
    public SafetyProgramId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SafetyProgramId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new SafetyProgramId using Uuid7.</summary>
    public static SafetyProgramId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a SafetyProgramId.</summary>
    public static SafetyProgramId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a SafetyProgramId.</summary>
    public static SafetyProgramId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}

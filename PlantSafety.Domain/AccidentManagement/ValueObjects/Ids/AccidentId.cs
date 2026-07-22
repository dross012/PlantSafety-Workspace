// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AccidentManagement
//  Layer:          Domain
//  File:           AccidentId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the Accident aggregate root.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;

namespace PlantSafety.Domain.AccidentManagement.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the Accident aggregate root.</summary>
public readonly record struct AccidentId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a AccidentId wrapping a non-empty GUID.</summary>
    public AccidentId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AccidentId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new AccidentId using Uuid7.</summary>
    public static AccidentId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a AccidentId.</summary>
    public static AccidentId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a AccidentId.</summary>
    public static AccidentId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
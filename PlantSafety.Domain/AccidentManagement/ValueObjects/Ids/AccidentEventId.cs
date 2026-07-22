// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AccidentManagement
//  Layer:          Domain
//  File:           AccidentEventId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for Accident domain events.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;

namespace PlantSafety.Domain.AccidentManagement.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for Accident domain events.</summary>
public readonly record struct AccidentEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a AccidentEventId wrapping a non-empty GUID.</summary>
    public AccidentEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AccidentEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new AccidentEventId using Uuid7.</summary>
    public static AccidentEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
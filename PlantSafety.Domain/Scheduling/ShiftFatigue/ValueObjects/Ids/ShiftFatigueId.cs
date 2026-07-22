// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.ShiftFatigue
//  Layer:          Domain
//  File:           ShiftFatigueId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity value object for the ShiftFatigue aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.ShiftFatigue.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the ShiftFatigue aggregate root.</summary>
public readonly record struct ShiftFatigueId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a ShiftFatigueId wrapping a non-empty GUID.</summary>
    public ShiftFatigueId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ShiftFatigueId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new ShiftFatigueId using Uuid7.</summary>
    public static ShiftFatigueId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a ShiftFatigueId.</summary>
    public static ShiftFatigueId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a ShiftFatigueId.</summary>
    public static ShiftFatigueId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
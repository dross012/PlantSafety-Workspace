// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.ShiftFatigue
//  Layer:          Domain
//  File:           ShiftFatigueEventId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity value object for ShiftFatigue domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.ShiftFatigue.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for ShiftFatigue domain events.</summary>
public readonly record struct ShiftFatigueEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a ShiftFatigueEventId wrapping a non-empty GUID.</summary>
    public ShiftFatigueEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ShiftFatigueEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new ShiftFatigueEventId using Uuid7.</summary>
    public static ShiftFatigueEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
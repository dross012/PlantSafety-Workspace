// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.SafetyValvesRelays
//  Layer:          Domain
//  File:           SafetyValvesRelaysEventId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for SafetyValvesRelays domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.SafetyValvesRelays.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for SafetyValvesRelays domain events.</summary>
public readonly record struct SafetyValvesRelaysEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a SafetyValvesRelaysEventId wrapping a non-empty GUID.</summary>
    public SafetyValvesRelaysEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SafetyValvesRelaysEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new SafetyValvesRelaysEventId using Uuid7.</summary>
    public static SafetyValvesRelaysEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.SafetyValvesRelays
//  Layer:          Domain
//  File:           SafetyValvesRelaysId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the SafetyValvesRelays aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.SafetyValvesRelays.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the SafetyValvesRelays aggregate root.</summary>
public readonly record struct SafetyValvesRelaysId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a SafetyValvesRelaysId wrapping a non-empty GUID.</summary>
    public SafetyValvesRelaysId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SafetyValvesRelaysId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new SafetyValvesRelaysId using Uuid7.</summary>
    public static SafetyValvesRelaysId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a SafetyValvesRelaysId.</summary>
    public static SafetyValvesRelaysId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a SafetyValvesRelaysId.</summary>
    public static SafetyValvesRelaysId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
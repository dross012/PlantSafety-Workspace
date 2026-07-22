// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.LotoSystems
//  Layer:          Domain
//  File:           LotoSystemsEventId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for LotoSystems domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.LotoSystems.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for LotoSystems domain events.</summary>
public readonly record struct LotoSystemsEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a LotoSystemsEventId wrapping a non-empty GUID.</summary>
    public LotoSystemsEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("LotoSystemsEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new LotoSystemsEventId using Uuid7.</summary>
    public static LotoSystemsEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
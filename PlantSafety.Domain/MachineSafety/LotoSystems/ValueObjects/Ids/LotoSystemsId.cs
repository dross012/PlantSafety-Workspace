// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.LotoSystems
//  Layer:          Domain
//  File:           LotoSystemsId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the LotoSystems aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.LotoSystems.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the LotoSystems aggregate root.</summary>
public readonly record struct LotoSystemsId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a LotoSystemsId wrapping a non-empty GUID.</summary>
    public LotoSystemsId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("LotoSystemsId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new LotoSystemsId using Uuid7.</summary>
    public static LotoSystemsId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a LotoSystemsId.</summary>
    public static LotoSystemsId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a LotoSystemsId.</summary>
    public static LotoSystemsId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
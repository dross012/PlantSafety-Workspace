// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPEInventory
//  Layer:          Domain
//  File:           PPEInventoryId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity value object for the PPEInventory aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPEInventory.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the PPEInventory aggregate root.</summary>
public readonly record struct PPEInventoryId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a PPEInventoryId wrapping a non-empty GUID.</summary>
    public PPEInventoryId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PPEInventoryId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new PPEInventoryId using Uuid7.</summary>
    public static PPEInventoryId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a PPEInventoryId.</summary>
    public static PPEInventoryId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a PPEInventoryId.</summary>
    public static PPEInventoryId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
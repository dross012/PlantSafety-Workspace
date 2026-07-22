// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPEInventory
//  Layer:          Domain
//  File:           PPEInventoryEventId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity value object for PPEInventory domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPEInventory.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for PPEInventory domain events.</summary>
public readonly record struct PPEInventoryEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a PPEInventoryEventId wrapping a non-empty GUID.</summary>
    public PPEInventoryEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PPEInventoryEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new PPEInventoryEventId using Uuid7.</summary>
    public static PPEInventoryEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
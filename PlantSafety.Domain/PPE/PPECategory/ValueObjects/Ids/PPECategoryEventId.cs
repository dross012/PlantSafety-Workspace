// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPECategory
//  Layer:          Domain
//  File:           PPECategoryEventId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity value object for PPECategory domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPECategory.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for PPECategory domain events.</summary>
public readonly record struct PPECategoryEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a PPECategoryEventId wrapping a non-empty GUID.</summary>
    public PPECategoryEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PPECategoryEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new PPECategoryEventId using Uuid7.</summary>
    public static PPECategoryEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
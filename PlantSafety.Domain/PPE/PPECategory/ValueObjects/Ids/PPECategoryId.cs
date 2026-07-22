// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPECategory
//  Layer:          Domain
//  File:           PPECategoryId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity value object for the PPECategory aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPECategory.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the PPECategory aggregate root.</summary>
public readonly record struct PPECategoryId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a PPECategoryId wrapping a non-empty GUID.</summary>
    public PPECategoryId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PPECategoryId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new PPECategoryId using Uuid7.</summary>
    public static PPECategoryId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a PPECategoryId.</summary>
    public static PPECategoryId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a PPECategoryId.</summary>
    public static PPECategoryId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
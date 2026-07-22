// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPEZoneMapping
//  Layer:          Domain
//  File:           PPEZoneMappingId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity value object for the PPEZoneMapping aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPEZoneMapping.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the PPEZoneMapping aggregate root.</summary>
public readonly record struct PPEZoneMappingId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a PPEZoneMappingId wrapping a non-empty GUID.</summary>
    public PPEZoneMappingId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PPEZoneMappingId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new PPEZoneMappingId using Uuid7.</summary>
    public static PPEZoneMappingId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a PPEZoneMappingId.</summary>
    public static PPEZoneMappingId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a PPEZoneMappingId.</summary>
    public static PPEZoneMappingId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
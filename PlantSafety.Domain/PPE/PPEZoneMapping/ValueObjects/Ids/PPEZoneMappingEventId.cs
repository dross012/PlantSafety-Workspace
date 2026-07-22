// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPEZoneMapping
//  Layer:          Domain
//  File:           PPEZoneMappingEventId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity value object for PPEZoneMapping domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPEZoneMapping.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for PPEZoneMapping domain events.</summary>
public readonly record struct PPEZoneMappingEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a PPEZoneMappingEventId wrapping a non-empty GUID.</summary>
    public PPEZoneMappingEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PPEZoneMappingEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new PPEZoneMappingEventId using Uuid7.</summary>
    public static PPEZoneMappingEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
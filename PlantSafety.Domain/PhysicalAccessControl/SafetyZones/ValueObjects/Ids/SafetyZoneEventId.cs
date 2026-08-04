// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.SafetyZones
//  Layer:          Domain
//  File:           SafetyZoneEventId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for SafetyZone domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.SafetyZones.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for SafetyZone domain events.</summary>
public readonly record struct SafetyZoneEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a SafetyZoneEventId wrapping a non-empty GUID.</summary>
    public SafetyZoneEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SafetyZoneEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new SafetyZoneEventId using Uuid7.</summary>
    public static SafetyZoneEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}

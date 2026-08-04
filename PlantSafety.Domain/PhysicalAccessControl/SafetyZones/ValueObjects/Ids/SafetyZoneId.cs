// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.SafetyZones
//  Layer:          Domain
//  File:           SafetyZoneId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the SafetyZone aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.SafetyZones.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the SafetyZone aggregate root.</summary>
public readonly record struct SafetyZoneId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a SafetyZoneId wrapping a non-empty GUID.</summary>
    public SafetyZoneId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SafetyZoneId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new SafetyZoneId using Uuid7.</summary>
    public static SafetyZoneId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a SafetyZoneId.</summary>
    public static SafetyZoneId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a SafetyZoneId.</summary>
    public static SafetyZoneId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}

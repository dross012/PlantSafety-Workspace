// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Placarding
//  Layer:          Domain
//  File:           HazmatContainerId.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the HazmatContainer aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.PHMSA.Placarding.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the HazmatContainer aggregate root.</summary>
public readonly record struct HazmatContainerId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a HazmatContainerId wrapping a non-empty GUID.</summary>
    public HazmatContainerId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("HazmatContainerId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new HazmatContainerId using Uuid7.</summary>
    public static HazmatContainerId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a HazmatContainerId.</summary>
    public static HazmatContainerId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a HazmatContainerId.</summary>
    public static HazmatContainerId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}

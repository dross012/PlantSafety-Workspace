// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Placarding
//  Layer:          Domain
//  File:           HazmatContainerEventId.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for HazmatContainer domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.PHMSA.Placarding.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for HazmatContainer domain events.</summary>
public readonly record struct HazmatContainerEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a HazmatContainerEventId wrapping a non-empty GUID.</summary>
    public HazmatContainerEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("HazmatContainerEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new HazmatContainerEventId using Uuid7.</summary>
    public static HazmatContainerEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EvacuationBottlenecks
//  Layer:          Domain
//  File:           EvacuationBottleneckId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the EvacuationBottleneck aggregate
//                  root. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the EvacuationBottleneck aggregate root.</summary>
public readonly record struct EvacuationBottleneckId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates an EvacuationBottleneckId wrapping a non-empty GUID.</summary>
    public EvacuationBottleneckId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("EvacuationBottleneckId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new EvacuationBottleneckId using Uuid7.</summary>
    public static EvacuationBottleneckId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as an EvacuationBottleneckId.</summary>
    public static EvacuationBottleneckId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into an EvacuationBottleneckId.</summary>
    public static EvacuationBottleneckId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EvacuationBottlenecks
//  Layer:          Domain
//  File:           EvacuationBottleneckEventId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for EvacuationBottleneck domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for EvacuationBottleneck domain events.</summary>
public readonly record struct EvacuationBottleneckEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates an EvacuationBottleneckEventId wrapping a non-empty GUID.</summary>
    public EvacuationBottleneckEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("EvacuationBottleneckEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new EvacuationBottleneckEventId using Uuid7.</summary>
    public static EvacuationBottleneckEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}

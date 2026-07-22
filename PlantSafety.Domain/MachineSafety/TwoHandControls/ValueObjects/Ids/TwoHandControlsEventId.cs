// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.TwoHandControls
//  Layer:          Domain
//  File:           TwoHandControlsEventId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for TwoHandControls domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.TwoHandControls.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for TwoHandControls domain events.</summary>
public readonly record struct TwoHandControlsEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a TwoHandControlsEventId wrapping a non-empty GUID.</summary>
    public TwoHandControlsEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("TwoHandControlsEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new TwoHandControlsEventId using Uuid7.</summary>
    public static TwoHandControlsEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
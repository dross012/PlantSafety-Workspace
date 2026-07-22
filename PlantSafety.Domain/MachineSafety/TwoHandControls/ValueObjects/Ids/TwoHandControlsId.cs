// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.TwoHandControls
//  Layer:          Domain
//  File:           TwoHandControlsId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the TwoHandControls aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.TwoHandControls.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the TwoHandControls aggregate root.</summary>
public readonly record struct TwoHandControlsId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a TwoHandControlsId wrapping a non-empty GUID.</summary>
    public TwoHandControlsId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("TwoHandControlsId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new TwoHandControlsId using Uuid7.</summary>
    public static TwoHandControlsId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a TwoHandControlsId.</summary>
    public static TwoHandControlsId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a TwoHandControlsId.</summary>
    public static TwoHandControlsId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
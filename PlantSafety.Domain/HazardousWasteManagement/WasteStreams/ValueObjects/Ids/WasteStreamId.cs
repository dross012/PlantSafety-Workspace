// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.WasteStreams
//  Layer:          Domain
//  File:           WasteStreamId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Strong-typed, Uuid7-backed identity for the WasteStream aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HazardousWasteManagement.WasteStreams.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the WasteStream aggregate root.</summary>
public readonly record struct WasteStreamId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a WasteStreamId wrapping a non-empty GUID.</summary>
    public WasteStreamId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("WasteStreamId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new WasteStreamId using Uuid7.</summary>
    public static WasteStreamId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a WasteStreamId.</summary>
    public static WasteStreamId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a WasteStreamId.</summary>
    public static WasteStreamId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}

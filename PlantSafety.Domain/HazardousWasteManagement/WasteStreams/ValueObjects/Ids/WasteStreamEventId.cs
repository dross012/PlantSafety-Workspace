// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.WasteStreams
//  Layer:          Domain
//  File:           WasteStreamEventId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Strong-typed, Uuid7-backed identity for WasteStream domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HazardousWasteManagement.WasteStreams.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for WasteStream domain events.</summary>
public readonly record struct WasteStreamEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a WasteStreamEventId wrapping a non-empty GUID.</summary>
    public WasteStreamEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("WasteStreamEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new WasteStreamEventId using Uuid7.</summary>
    public static WasteStreamEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}

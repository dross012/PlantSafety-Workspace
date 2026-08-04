// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.WasteContainers
//  Layer:          Domain
//  File:           WasteContainerEventId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Strong-typed, Uuid7-backed identity for WasteContainer domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HazardousWasteManagement.WasteContainers.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for WasteContainer domain events.</summary>
public readonly record struct WasteContainerEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a WasteContainerEventId wrapping a non-empty GUID.</summary>
    public WasteContainerEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("WasteContainerEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new WasteContainerEventId using Uuid7.</summary>
    public static WasteContainerEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.WasteContainers
//  Layer:          Domain
//  File:           WasteContainerId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Strong-typed, Uuid7-backed identity for the WasteContainer aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HazardousWasteManagement.WasteContainers.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the WasteContainer aggregate root.</summary>
public readonly record struct WasteContainerId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a WasteContainerId wrapping a non-empty GUID.</summary>
    public WasteContainerId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("WasteContainerId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new WasteContainerId using Uuid7.</summary>
    public static WasteContainerId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a WasteContainerId.</summary>
    public static WasteContainerId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a WasteContainerId.</summary>
    public static WasteContainerId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}

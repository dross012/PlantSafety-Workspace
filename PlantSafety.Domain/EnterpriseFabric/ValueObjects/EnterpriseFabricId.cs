// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnterpriseFabric
//  Layer:          Domain
//  File:           EnterpriseFabricId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Strong-typed Uuid7-backed identity for the EnterpriseFabric aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnterpriseFabric.ValueObjects;

/// <summary>Strong-typed, Uuid7-backed identity for the EnterpriseFabric aggregate root.</summary>
public readonly record struct EnterpriseFabricId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a EnterpriseFabricId wrapping a non-empty GUID.</summary>
    public EnterpriseFabricId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("EnterpriseFabricId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new EnterpriseFabricId using Uuid7.</summary>
    public static EnterpriseFabricId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a EnterpriseFabricId.</summary>
    public static EnterpriseFabricId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a EnterpriseFabricId.</summary>
    public static EnterpriseFabricId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}

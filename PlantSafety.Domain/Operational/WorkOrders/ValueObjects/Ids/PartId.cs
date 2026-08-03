// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.WorkOrders
//  Layer:          Domain
//  File:           PartId.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the Part (inventory) aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.WorkOrders.ValueObjects.Ids;

/// <summary>Strong-typed identity for the Part aggregate root.</summary>
public readonly record struct PartId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a PartId wrapping a non-empty GUID.</summary>
    public PartId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PartId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new PartId using Uuid7.</summary>
    public static PartId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a PartId.</summary>
    public static PartId FromGuid(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}

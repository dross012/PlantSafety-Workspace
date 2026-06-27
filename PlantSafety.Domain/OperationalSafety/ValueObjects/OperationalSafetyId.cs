// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         OperationalSafety
//  Layer:          Domain
//  File:           OperationalSafetyId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Strong-typed Uuid7-backed identity for the OperationalSafety aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.OperationalSafety.ValueObjects;

/// <summary>Strong-typed, Uuid7-backed identity for the OperationalSafety aggregate root.</summary>
public readonly record struct OperationalSafetyId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a OperationalSafetyId wrapping a non-empty GUID.</summary>
    public OperationalSafetyId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("OperationalSafetyId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new OperationalSafetyId using Uuid7.</summary>
    public static OperationalSafetyId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a OperationalSafetyId.</summary>
    public static OperationalSafetyId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a OperationalSafetyId.</summary>
    public static OperationalSafetyId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}

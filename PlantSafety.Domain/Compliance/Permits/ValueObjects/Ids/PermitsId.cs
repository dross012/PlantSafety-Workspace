// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Permits
//  Layer:          Domain
//  File:           PermitsId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Permits.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the Permits aggregate root.</summary>
public readonly record struct PermitsId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a PermitsId wrapping a non-empty GUID.</summary>
    public PermitsId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PermitsId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new PermitsId using Uuid7.</summary>
    public static PermitsId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a PermitsId.</summary>
    public static PermitsId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a PermitsId.</summary>
    public static PermitsId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}

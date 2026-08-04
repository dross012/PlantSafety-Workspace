// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.AccessEvents
//  Layer:          Domain
//  File:           AccessEventId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the AccessEvent aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.AccessEvents.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the AccessEvent aggregate root.</summary>
public readonly record struct AccessEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates an AccessEventId wrapping a non-empty GUID.</summary>
    public AccessEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AccessEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new AccessEventId using Uuid7.</summary>
    public static AccessEventId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as an AccessEventId.</summary>
    public static AccessEventId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into an AccessEventId.</summary>
    public static AccessEventId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}

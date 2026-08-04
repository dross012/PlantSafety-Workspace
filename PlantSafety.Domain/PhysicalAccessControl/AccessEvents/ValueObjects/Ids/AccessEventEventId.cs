// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.AccessEvents
//  Layer:          Domain
//  File:           AccessEventEventId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for AccessEvent domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.AccessEvents.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for AccessEvent domain events.</summary>
public readonly record struct AccessEventEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates an AccessEventEventId wrapping a non-empty GUID.</summary>
    public AccessEventEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AccessEventEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new AccessEventEventId using Uuid7.</summary>
    public static AccessEventEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.AccessPoints
//  Layer:          Domain
//  File:           AccessPointId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the AccessPoint aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.AccessPoints.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the AccessPoint aggregate root.</summary>
public readonly record struct AccessPointId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates an AccessPointId wrapping a non-empty GUID.</summary>
    public AccessPointId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AccessPointId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new AccessPointId using Uuid7.</summary>
    public static AccessPointId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as an AccessPointId.</summary>
    public static AccessPointId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into an AccessPointId.</summary>
    public static AccessPointId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}

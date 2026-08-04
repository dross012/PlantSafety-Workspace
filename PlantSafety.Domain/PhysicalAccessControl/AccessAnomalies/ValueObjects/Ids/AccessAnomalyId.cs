// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.AccessAnomalies
//  Layer:          Domain
//  File:           AccessAnomalyId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the AccessAnomaly aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the AccessAnomaly aggregate root.</summary>
public readonly record struct AccessAnomalyId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates an AccessAnomalyId wrapping a non-empty GUID.</summary>
    public AccessAnomalyId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AccessAnomalyId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new AccessAnomalyId using Uuid7.</summary>
    public static AccessAnomalyId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as an AccessAnomalyId.</summary>
    public static AccessAnomalyId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into an AccessAnomalyId.</summary>
    public static AccessAnomalyId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}

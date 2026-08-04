// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.RestrictedZonePolicies
//  Layer:          Domain
//  File:           RestrictedZonePolicyEventId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for RestrictedZonePolicy domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.RestrictedZonePolicies.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for RestrictedZonePolicy domain events.</summary>
public readonly record struct RestrictedZonePolicyEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a RestrictedZonePolicyEventId wrapping a non-empty GUID.</summary>
    public RestrictedZonePolicyEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("RestrictedZonePolicyEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new RestrictedZonePolicyEventId using Uuid7.</summary>
    public static RestrictedZonePolicyEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}

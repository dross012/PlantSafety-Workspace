// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.RestrictedZonePolicies
//  Layer:          Domain
//  File:           RestrictedZonePolicyId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the RestrictedZonePolicy aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.RestrictedZonePolicies.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the RestrictedZonePolicy aggregate root.</summary>
public readonly record struct RestrictedZonePolicyId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a RestrictedZonePolicyId wrapping a non-empty GUID.</summary>
    public RestrictedZonePolicyId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("RestrictedZonePolicyId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new RestrictedZonePolicyId using Uuid7.</summary>
    public static RestrictedZonePolicyId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a RestrictedZonePolicyId.</summary>
    public static RestrictedZonePolicyId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a RestrictedZonePolicyId.</summary>
    public static RestrictedZonePolicyId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}

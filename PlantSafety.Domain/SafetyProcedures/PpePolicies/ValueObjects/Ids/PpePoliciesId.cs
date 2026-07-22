// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.PpePolicies
//  Layer:          Domain
//  File:           PpePoliciesId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the PpePolicies aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.PpePolicies.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the PpePolicies aggregate root.</summary>
public readonly record struct PpePoliciesId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a PpePoliciesId wrapping a non-empty GUID.</summary>
    public PpePoliciesId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PpePoliciesId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new PpePoliciesId using Uuid7.</summary>
    public static PpePoliciesId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a PpePoliciesId.</summary>
    public static PpePoliciesId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a PpePoliciesId.</summary>
    public static PpePoliciesId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.PpePolicies
//  Layer:          Domain
//  File:           PpePoliciesEventId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for PpePolicies domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.PpePolicies.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for PpePolicies domain events.</summary>
public readonly record struct PpePoliciesEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a PpePoliciesEventId wrapping a non-empty GUID.</summary>
    public PpePoliciesEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PpePoliciesEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new PpePoliciesEventId using Uuid7.</summary>
    public static PpePoliciesEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
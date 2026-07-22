// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyPolicies
//  Layer:          Domain
//  File:           SafetyPolicyId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the SafetyPolicy aggregate root.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;

namespace PlantSafety.Domain.SafetyPolicies.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the SafetyPolicy aggregate root.</summary>
public readonly record struct SafetyPolicyId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a SafetyPolicyId wrapping a non-empty GUID.</summary>
    public SafetyPolicyId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SafetyPolicyId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new SafetyPolicyId using Uuid7.</summary>
    public static SafetyPolicyId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a SafetyPolicyId.</summary>
    public static SafetyPolicyId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a SafetyPolicyId.</summary>
    public static SafetyPolicyId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
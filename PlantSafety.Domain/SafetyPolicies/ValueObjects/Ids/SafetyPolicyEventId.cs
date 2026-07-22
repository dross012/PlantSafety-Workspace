// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyPolicies
//  Layer:          Domain
//  File:           SafetyPolicyEventId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for SafetyPolicy domain events.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;

namespace PlantSafety.Domain.SafetyPolicies.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for SafetyPolicy domain events.</summary>
public readonly record struct SafetyPolicyEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a SafetyPolicyEventId wrapping a non-empty GUID.</summary>
    public SafetyPolicyEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SafetyPolicyEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new SafetyPolicyEventId using Uuid7.</summary>
    public static SafetyPolicyEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
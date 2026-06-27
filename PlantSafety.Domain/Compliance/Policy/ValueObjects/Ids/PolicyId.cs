// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Policy
//  Layer:          Domain
//  File:           PolicyId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Policy.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the Policy aggregate root.</summary>
public readonly record struct PolicyId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a PolicyId wrapping a non-empty GUID.</summary>
    public PolicyId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PolicyId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new PolicyId using Uuid7.</summary>
    public static PolicyId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a PolicyId.</summary>
    public static PolicyId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a PolicyId.</summary>
    public static PolicyId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}

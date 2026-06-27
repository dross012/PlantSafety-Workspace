// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Policy
//  Layer:          Domain
//  File:           PolicyEventId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Policy.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for Policy domain events.</summary>
public readonly record struct PolicyEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a PolicyEventId wrapping a non-empty GUID.</summary>
    public PolicyEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PolicyEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new PolicyEventId using Uuid7.</summary>
    public static PolicyEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}

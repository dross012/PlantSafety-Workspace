// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Governance
//  Layer:          Domain
//  File:           GovernanceId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Strong-typed Uuid7-backed identity for the Governance aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Governance.ValueObjects;

/// <summary>Strong-typed, Uuid7-backed identity for the Governance aggregate root.</summary>
public readonly record struct GovernanceId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a GovernanceId wrapping a non-empty GUID.</summary>
    public GovernanceId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("GovernanceId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new GovernanceId using Uuid7.</summary>
    public static GovernanceId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a GovernanceId.</summary>
    public static GovernanceId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a GovernanceId.</summary>
    public static GovernanceId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}

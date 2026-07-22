// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         RiskManagement
//  Layer:          Domain
//  File:           RiskId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the Risk aggregate root.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;

namespace PlantSafety.Domain.RiskManagement.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the Risk aggregate root.</summary>
public readonly record struct RiskId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a RiskId wrapping a non-empty GUID.</summary>
    public RiskId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("RiskId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new RiskId using Uuid7.</summary>
    public static RiskId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a RiskId.</summary>
    public static RiskId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a RiskId.</summary>
    public static RiskId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
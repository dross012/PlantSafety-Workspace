// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         RiskManagement
//  Layer:          Domain
//  File:           RiskEventId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for Risk domain events.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;

namespace PlantSafety.Domain.RiskManagement.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for Risk domain events.</summary>
public readonly record struct RiskEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a RiskEventId wrapping a non-empty GUID.</summary>
    public RiskEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("RiskEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new RiskEventId using Uuid7.</summary>
    public static RiskEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
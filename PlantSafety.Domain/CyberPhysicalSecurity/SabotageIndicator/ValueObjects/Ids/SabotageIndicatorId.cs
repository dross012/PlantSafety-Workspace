// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.SabotageIndicator
//  Layer:          Domain
//  File:           SabotageIndicatorId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity for the SabotageIndicator aggregate root (Sabotage
//                  Indicator Correlation subdomain of CyberPhysicalSecurity).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the SabotageIndicator aggregate root.</summary>
public readonly record struct SabotageIndicatorId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a SabotageIndicatorId wrapping a non-empty GUID.</summary>
    public SabotageIndicatorId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SabotageIndicatorId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new SabotageIndicatorId using Uuid7.</summary>
    public static SabotageIndicatorId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a SabotageIndicatorId.</summary>
    public static SabotageIndicatorId FromGuid(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}

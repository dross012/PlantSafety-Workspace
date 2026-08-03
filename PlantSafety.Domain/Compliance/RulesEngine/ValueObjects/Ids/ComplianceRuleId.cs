// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RulesEngine
//  Layer:          Domain
//  File:           ComplianceRuleId.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for a registered compliance rule
//                  (Safety, Regulatory, or Internal Policy) evaluated by the Compliance
//                  Rules Engine. Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;

namespace PlantSafety.Domain.Compliance.RulesEngine.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for a registered compliance rule.</summary>
public readonly record struct ComplianceRuleId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a ComplianceRuleId wrapping a non-empty GUID.</summary>
    public ComplianceRuleId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ComplianceRuleId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new ComplianceRuleId using Uuid7.</summary>
    public static ComplianceRuleId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a ComplianceRuleId.</summary>
    public static ComplianceRuleId FromGuid(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}

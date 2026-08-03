// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RulesEngine
//  Layer:          Domain
//  File:           ComplianceRuleResultId.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for a single ComplianceRuleResult
//                  child entity recorded within a ComplianceEvaluation. Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;

namespace PlantSafety.Domain.Compliance.RulesEngine.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for a ComplianceRuleResult entity.</summary>
public readonly record struct ComplianceRuleResultId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a ComplianceRuleResultId wrapping a non-empty GUID.</summary>
    public ComplianceRuleResultId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ComplianceRuleResultId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new ComplianceRuleResultId using Uuid7.</summary>
    public static ComplianceRuleResultId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a ComplianceRuleResultId.</summary>
    public static ComplianceRuleResultId FromGuid(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}

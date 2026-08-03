// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RulesEngine
//  Layer:          Domain
//  File:           ComplianceEvaluationId.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the ComplianceEvaluation
//                  aggregate root (one deterministic rule-pipeline run against a target
//                  entity). Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;

namespace PlantSafety.Domain.Compliance.RulesEngine.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the ComplianceEvaluation aggregate root.</summary>
public readonly record struct ComplianceEvaluationId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a ComplianceEvaluationId wrapping a non-empty GUID.</summary>
    public ComplianceEvaluationId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ComplianceEvaluationId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new ComplianceEvaluationId using Uuid7.</summary>
    public static ComplianceEvaluationId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a ComplianceEvaluationId.</summary>
    public static ComplianceEvaluationId FromGuid(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}

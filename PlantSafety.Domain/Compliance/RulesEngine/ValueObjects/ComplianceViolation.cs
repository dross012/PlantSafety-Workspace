// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RulesEngine
//  Layer:          Domain
//  File:           ComplianceViolation.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Immutable value object describing a single violation detail raised by a
//                  failing ComplianceRuleResult. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.RulesEngine.Enums;

namespace PlantSafety.Domain.Compliance.RulesEngine.ValueObjects;

/// <summary>Immutable description of a single compliance violation detail.</summary>
public sealed record ComplianceViolation
{
    /// <summary>Stable machine-readable violation code (e.g. "Permit.Expired").</summary>
    public required string ViolationCode { get; init; }

    /// <summary>Human-readable description of the violation, safe for printed reports.</summary>
    public required string Description { get; init; }

    /// <summary>Severity of this specific violation.</summary>
    public required ComplianceSeverity Severity { get; init; }

    /// <summary>Optional free-form remediation guidance.</summary>
    public string? RemediationGuidance { get; init; }

    /// <summary>Creates a new ComplianceViolation.</summary>
    public static ComplianceViolation Create(string violationCode, string description, ComplianceSeverity severity, string? remediationGuidance = null)
        => new()
        {
            ViolationCode = (violationCode ?? string.Empty).Trim(),
            Description = (description ?? string.Empty).Trim(),
            Severity = severity,
            RemediationGuidance = remediationGuidance,
        };
}

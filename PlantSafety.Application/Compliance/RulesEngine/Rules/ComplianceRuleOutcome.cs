// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: Deterministic pass/fail outcome returned by every IComplianceRule invocation,
//   carrying violation details when the rule failed.
// SchemaVersion: 1

using PlantSafety.Domain.Compliance.RulesEngine.Enums;

namespace PlantSafety.Application.Compliance.RulesEngine.Rules;

/// <summary>Deterministic pass/fail outcome of a single compliance rule evaluation.</summary>
public sealed record ComplianceRuleOutcome
{
    /// <summary>Whether the rule passed for the evaluated target.</summary>
    public required bool Passed { get; init; }

    /// <summary>Violation details raised by the rule; empty when <see cref="Passed"/> is <see langword="true"/>.</summary>
    public IReadOnlyList<ComplianceRuleViolationDetail> Violations { get; init; } = [];

    /// <summary>Creates a passing outcome.</summary>
    public static ComplianceRuleOutcome Pass() => new() { Passed = true };

    /// <summary>Creates a failing outcome carrying one or more violation details.</summary>
    public static ComplianceRuleOutcome Fail(params ComplianceRuleViolationDetail[] violations)
        => new() { Passed = false, Violations = violations };
}

/// <summary>A single violation detail raised by a failing compliance rule.</summary>
/// <param name="ViolationCode">Stable machine-readable violation code.</param>
/// <param name="Description">Human-readable description, safe for printed reports.</param>
/// <param name="Severity">Severity of this specific violation.</param>
/// <param name="RemediationGuidance">Optional free-form remediation guidance.</param>
public sealed record ComplianceRuleViolationDetail(
    string ViolationCode,
    string Description,
    ComplianceSeverity Severity,
    string? RemediationGuidance = null);

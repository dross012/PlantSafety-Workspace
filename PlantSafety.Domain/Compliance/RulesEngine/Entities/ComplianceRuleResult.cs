// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RulesEngine
//  Layer:          Domain
//  File:           ComplianceRuleResult.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Child entity of ComplianceEvaluation recording the pass/fail outcome of a
//                  single named compliance rule (Safety, Regulatory, or InternalPolicy), plus
//                  its violation details when failed. Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Compliance.RulesEngine.Enums;
using PlantSafety.Domain.Compliance.RulesEngine.ValueObjects;
using PlantSafety.Domain.Compliance.RulesEngine.ValueObjects.Ids;

namespace PlantSafety.Domain.Compliance.RulesEngine.Entities;

/// <summary>Records the outcome of a single compliance rule evaluated within a ComplianceEvaluation.</summary>
public sealed class ComplianceRuleResult
{
    /// <summary>Strong-typed identity of this rule result.</summary>
    public ComplianceRuleResultId ComplianceRuleResultId { get; private set; }

    /// <summary>Owning evaluation.</summary>
    public ComplianceEvaluationId ComplianceEvaluationId { get; private set; }

    /// <summary>Identity of the rule that produced this result.</summary>
    public ComplianceRuleId RuleId { get; private set; }

    /// <summary>Stable machine-readable rule code (e.g. "Safety.ControlMeasureJustificationRequired").</summary>
    public string RuleCode { get; private set; } = string.Empty;

    /// <summary>Human-readable rule name.</summary>
    public string RuleName { get; private set; } = string.Empty;

    /// <summary>Governing category pipeline this rule belongs to.</summary>
    public ComplianceRuleCategory Category { get; private set; } = ComplianceRuleCategory.Default;

    /// <summary>Whether the rule passed for the evaluated target.</summary>
    public bool Passed { get; private set; }

    private readonly List<ComplianceViolation> _violations = [];

    /// <summary>Violation details raised by this rule; empty when <see cref="Passed"/> is <see langword="true"/>.</summary>
    public IReadOnlyList<ComplianceViolation> Violations => _violations.AsReadOnly();

    /// <summary>UTC instant the rule was evaluated.</summary>
    public UtcTimestamp EvaluatedAtUtc { get; private set; }

    // For rehydration only.
    private ComplianceRuleResult() { }

    /// <summary>Creates a passing rule result.</summary>
    public static ComplianceRuleResult Pass(
        ComplianceEvaluationId evaluationId, ComplianceRuleId ruleId, string ruleCode, string ruleName,
        ComplianceRuleCategory category, UtcTimestamp evaluatedAtUtc)
        => new()
        {
            ComplianceRuleResultId = ComplianceRuleResultId.New(),
            ComplianceEvaluationId = evaluationId,
            RuleId = ruleId,
            RuleCode = (ruleCode ?? string.Empty).Trim(),
            RuleName = (ruleName ?? string.Empty).Trim(),
            Category = category,
            Passed = true,
            EvaluatedAtUtc = evaluatedAtUtc,
        };

    /// <summary>Creates a failing rule result carrying one or more violation details.</summary>
    public static ComplianceRuleResult Fail(
        ComplianceEvaluationId evaluationId, ComplianceRuleId ruleId, string ruleCode, string ruleName,
        ComplianceRuleCategory category, UtcTimestamp evaluatedAtUtc, IEnumerable<ComplianceViolation> violations)
    {
        var result = new ComplianceRuleResult
        {
            ComplianceRuleResultId = ComplianceRuleResultId.New(),
            ComplianceEvaluationId = evaluationId,
            RuleId = ruleId,
            RuleCode = (ruleCode ?? string.Empty).Trim(),
            RuleName = (ruleName ?? string.Empty).Trim(),
            Category = category,
            Passed = false,
            EvaluatedAtUtc = evaluatedAtUtc,
        };
        result._violations.AddRange(violations);
        return result;
    }

    /// <summary>Highest-severity violation carried by this result, or <see langword="null"/> when passed.</summary>
    public ComplianceViolation? MostSevereViolation()
        => _violations.OrderByDescending(v => v.Severity.Rank).FirstOrDefault();
}

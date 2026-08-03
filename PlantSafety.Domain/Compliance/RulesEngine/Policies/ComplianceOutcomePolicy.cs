// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RulesEngine
//  Layer:          Domain
//  File:           ComplianceOutcomePolicy.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Deterministic policy that derives the overall Compliant/NonCompliant
//                  outcome of a ComplianceEvaluation from its recorded rule results. Any
//                  High or Critical severity violation always fails the evaluation; two or more
//                  Medium violations also fail it; Low/Advisory violations alone do not.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.RulesEngine.Entities;
using PlantSafety.Domain.Compliance.RulesEngine.Enums;

namespace PlantSafety.Domain.Compliance.RulesEngine.Policies;

/// <summary>Determines the overall pass/fail outcome of a ComplianceEvaluation from its rule results.</summary>
public static class ComplianceOutcomePolicy
{
    private const int MediumViolationFailureThreshold = 2;

    /// <summary>Returns the overall <see cref="ComplianceEvaluationStatus"/> for the given rule results.</summary>
    public static ComplianceEvaluationStatus DetermineOutcome(IReadOnlyList<ComplianceRuleResult> ruleResults)
    {
        var violations = ruleResults.SelectMany(r => r.Violations).ToList();

        var hasHighOrCritical = violations.Any(v => v.Severity.Rank >= ComplianceSeverity.High.Rank);
        var mediumCount = violations.Count(v => v.Severity == ComplianceSeverity.Medium);

        return hasHighOrCritical || mediumCount >= MediumViolationFailureThreshold
            ? ComplianceEvaluationStatus.NonCompliant
            : ComplianceEvaluationStatus.Compliant;
    }
}

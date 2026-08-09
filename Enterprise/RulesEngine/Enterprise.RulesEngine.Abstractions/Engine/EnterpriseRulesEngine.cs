// ============================================================================
//  Project:        Enterprise.RulesEngine
//  Module:         Abstractions.Engine
//  File:           EnterpriseRulesEngine.cs
//  Created:        2026-08-08
//  Description:    Phase D.5 (Path D: Enterprise-Scale Safety Platform
//                  Modernization): evaluates a set of EnterpriseRule
//                  instances against a fact set, deterministically, in
//                  the order the rules were supplied. Pure function, no
//                  side effects, no reflection -- supports every rule
//                  category the spec calls out (Hazard/Severity/
//                  Regulatory/Workflow/Escalation) uniformly, since
//                  EnterpriseRuleType is just a label on an otherwise
//                  identical condition/action rule.
// ============================================================================
using Enterprise.RulesEngine.Abstractions.Actions;
using Enterprise.RulesEngine.Abstractions.Enums;
using Enterprise.RulesEngine.Abstractions.Rules;

namespace Enterprise.RulesEngine.Abstractions.Engine;

/// <summary>Deterministically evaluates enterprise rules against a fact set.</summary>
public static class EnterpriseRulesEngine
{
    /// <summary>Evaluates every rule in <paramref name="rules"/> against <paramref name="facts"/>, in order.</summary>
    public static IReadOnlyList<EnterpriseRuleEvaluationResult> Evaluate(IReadOnlyList<EnterpriseRule> rules, IReadOnlyDictionary<string, string> facts) =>
        rules.Select(rule =>
        {
            bool matched = rule.Matches(facts);
            IReadOnlyList<EnterpriseRuleAction> triggeredActions = matched ? rule.Actions : [];
            return new EnterpriseRuleEvaluationResult(rule.RuleId, rule.Name, matched, triggeredActions);
        }).ToArray();

    /// <summary>Evaluates every rule of a specific <see cref="EnterpriseRuleType"/> in <paramref name="rules"/> against <paramref name="facts"/>, in order.</summary>
    public static IReadOnlyList<EnterpriseRuleEvaluationResult> EvaluateByType(IReadOnlyList<EnterpriseRule> rules, EnterpriseRuleType ruleType, IReadOnlyDictionary<string, string> facts) =>
        Evaluate(rules.Where(r => r.RuleType == ruleType).ToArray(), facts);
}

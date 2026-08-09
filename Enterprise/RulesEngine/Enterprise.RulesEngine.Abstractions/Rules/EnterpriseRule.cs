// ============================================================================
//  Project:        Enterprise.RulesEngine
//  Module:         Abstractions.Rules
//  File:           EnterpriseRule.cs
//  Created:        2026-08-08
//  Description:    Phase D.5: a single deterministic enterprise rule --
//                  every one of its Conditions must be satisfied (AND
//                  semantics, no invented OR/NOT combinators) for its
//                  Actions to trigger. Disabled rules (IsEnabled = false)
//                  never match, regardless of their conditions.
// ============================================================================
using Enterprise.RulesEngine.Abstractions.Actions;
using Enterprise.RulesEngine.Abstractions.Conditions;
using Enterprise.RulesEngine.Abstractions.Enums;
using Enterprise.RulesEngine.Abstractions.Ids;

namespace Enterprise.RulesEngine.Abstractions.Rules;

/// <summary>A single deterministic enterprise rule: conditions (AND semantics) plus the actions they trigger.</summary>
public sealed record EnterpriseRule(
    EnterpriseRuleId RuleId,
    string Name,
    EnterpriseRuleType RuleType,
    IReadOnlyList<EnterpriseRuleCondition> Conditions,
    IReadOnlyList<EnterpriseRuleAction> Actions,
    bool IsEnabled = true)
{
    /// <summary>True if this rule is enabled and every one of its conditions is satisfied by the given facts.</summary>
    public bool Matches(IReadOnlyDictionary<string, string> facts) =>
        IsEnabled && Conditions.Count > 0 && Conditions.All(condition => condition.IsSatisfiedBy(facts));
}

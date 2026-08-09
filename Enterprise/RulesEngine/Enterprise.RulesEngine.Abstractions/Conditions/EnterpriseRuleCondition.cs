// ============================================================================
//  Project:        Enterprise.RulesEngine
//  Module:         Abstractions.Conditions
//  File:           EnterpriseRuleCondition.cs
//  Created:        2026-08-08
//  Description:    Phase D.5: a single deterministic condition an
//                  EnterpriseRule evaluates against a caller-supplied fact
//                  dictionary. Facts are plain string key/value pairs
//                  (mirrors Shared.FacilityDomain's HazardInferenceRules
//                  operating on caller-supplied strings) rather than typed
//                  domain objects, keeping Enterprise.RulesEngine decoupled
//                  from every upstream domain project. A missing fact key
//                  never matches -- it is never treated as a wildcard.
// ============================================================================
using Enterprise.RulesEngine.Abstractions.Enums;

namespace Enterprise.RulesEngine.Abstractions.Conditions;

/// <summary>A single deterministic condition, evaluated against a fact dictionary.</summary>
public sealed record EnterpriseRuleCondition(
    string FactKey,
    EnterpriseRuleComparisonOperator Operator,
    string ComparisonValue)
{
    /// <summary>Evaluates this condition against the given facts. A missing fact key never matches.</summary>
    public bool IsSatisfiedBy(IReadOnlyDictionary<string, string> facts)
    {
        if (!facts.TryGetValue(FactKey, out string? factValue))
        {
            return false;
        }

        return Operator switch
        {
            EnterpriseRuleComparisonOperator.Equals => string.Equals(factValue, ComparisonValue, StringComparison.Ordinal),
            EnterpriseRuleComparisonOperator.NotEquals => !string.Equals(factValue, ComparisonValue, StringComparison.Ordinal),
            EnterpriseRuleComparisonOperator.Contains => factValue.Contains(ComparisonValue, StringComparison.Ordinal),
            EnterpriseRuleComparisonOperator.GreaterThan => TryCompareNumerically(factValue, ComparisonValue, out int cmp) && cmp > 0,
            EnterpriseRuleComparisonOperator.LessThan => TryCompareNumerically(factValue, ComparisonValue, out int cmp) && cmp < 0,
            _ => false,
        };
    }

    private static bool TryCompareNumerically(string factValue, string comparisonValue, out int comparison)
    {
        comparison = 0;
        if (!double.TryParse(factValue, out double factNumber) || !double.TryParse(comparisonValue, out double comparisonNumber))
        {
            return false;
        }

        comparison = factNumber.CompareTo(comparisonNumber);
        return true;
    }
}

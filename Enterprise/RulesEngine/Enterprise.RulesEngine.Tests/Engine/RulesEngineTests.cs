// ============================================================================
//  Project:        Enterprise.RulesEngine.Tests
//  File:           RulesEngineTests.cs
//  Created:        2026-08-08
//  Description:    Phase D.5 unit tests for EnterpriseRuleCondition/
//                  EnterpriseRule/EnterpriseRulesEngine: each comparison
//                  operator, AND semantics across multiple conditions,
//                  disabled-rule handling, missing-fact-key handling, and
//                  EvaluateByType filtering.
// ============================================================================
using Enterprise.RulesEngine.Abstractions.Actions;
using Enterprise.RulesEngine.Abstractions.Conditions;
using Enterprise.RulesEngine.Abstractions.Engine;
using Enterprise.RulesEngine.Abstractions.Enums;
using Enterprise.RulesEngine.Abstractions.Ids;
using Enterprise.RulesEngine.Abstractions.Rules;

using FluentAssertions;

using Xunit;

namespace Enterprise.RulesEngine.Tests.Engine;

public sealed class RulesEngineTests
{
    [Theory]
    [InlineData(EnterpriseRuleComparisonOperator.Equals, "Fire", "Fire", true)]
    [InlineData(EnterpriseRuleComparisonOperator.Equals, "Fire", "Spill", false)]
    [InlineData(EnterpriseRuleComparisonOperator.NotEquals, "Fire", "Spill", true)]
    [InlineData(EnterpriseRuleComparisonOperator.Contains, "ChemicalSpill", "Spill", true)]
    [InlineData(EnterpriseRuleComparisonOperator.GreaterThan, "5", "3", true)]
    [InlineData(EnterpriseRuleComparisonOperator.LessThan, "2", "3", true)]
    [InlineData(EnterpriseRuleComparisonOperator.GreaterThan, "not-a-number", "3", false)]
    public void EnterpriseRuleCondition_IsSatisfiedBy_EvaluatesEachOperator(EnterpriseRuleComparisonOperator op, string factValue, string comparisonValue, bool expected)
    {
        var condition = new EnterpriseRuleCondition("HazardType", op, comparisonValue);
        var facts = new Dictionary<string, string> { ["HazardType"] = factValue };

        condition.IsSatisfiedBy(facts).Should().Be(expected);
    }

    [Fact]
    public void EnterpriseRuleCondition_MissingFactKey_NeverMatches()
    {
        var condition = new EnterpriseRuleCondition("HazardType", EnterpriseRuleComparisonOperator.Equals, "Fire");

        condition.IsSatisfiedBy(new Dictionary<string, string>()).Should().BeFalse();
    }

    [Fact]
    public void EnterpriseRule_Matches_RequiresAllConditions()
    {
        var rule = new EnterpriseRule(
            EnterpriseRuleId.New(),
            "High severity fire",
            EnterpriseRuleType.Hazard,
            [
                new EnterpriseRuleCondition("HazardType", EnterpriseRuleComparisonOperator.Equals, "Fire"),
                new EnterpriseRuleCondition("Severity", EnterpriseRuleComparisonOperator.Equals, "Severe"),
            ],
            [new EnterpriseRuleAction("EVACUATE", "Trigger evacuation.")]);

        var allMatch = new Dictionary<string, string> { ["HazardType"] = "Fire", ["Severity"] = "Severe" };
        var partialMatch = new Dictionary<string, string> { ["HazardType"] = "Fire", ["Severity"] = "Minor" };

        rule.Matches(allMatch).Should().BeTrue();
        rule.Matches(partialMatch).Should().BeFalse();
    }

    [Fact]
    public void EnterpriseRule_DisabledRule_NeverMatches()
    {
        var rule = new EnterpriseRule(
            EnterpriseRuleId.New(),
            "Disabled rule",
            EnterpriseRuleType.Severity,
            [new EnterpriseRuleCondition("Severity", EnterpriseRuleComparisonOperator.Equals, "Severe")],
            [new EnterpriseRuleAction("ESCALATE", "Escalate.")],
            IsEnabled: false);

        rule.Matches(new Dictionary<string, string> { ["Severity"] = "Severe" }).Should().BeFalse();
    }

    [Fact]
    public void EnterpriseRulesEngine_Evaluate_ReturnsTriggeredActionsOnlyForMatchedRules()
    {
        var matchingRule = new EnterpriseRule(
            EnterpriseRuleId.New(), "Matching", EnterpriseRuleType.Hazard,
            [new EnterpriseRuleCondition("HazardType", EnterpriseRuleComparisonOperator.Equals, "Fire")],
            [new EnterpriseRuleAction("EVACUATE", "Evacuate.")]);
        var nonMatchingRule = new EnterpriseRule(
            EnterpriseRuleId.New(), "NonMatching", EnterpriseRuleType.Hazard,
            [new EnterpriseRuleCondition("HazardType", EnterpriseRuleComparisonOperator.Equals, "Spill")],
            [new EnterpriseRuleAction("CONTAIN", "Contain.")]);
        var facts = new Dictionary<string, string> { ["HazardType"] = "Fire" };

        IReadOnlyList<EnterpriseRuleEvaluationResult> results = EnterpriseRulesEngine.Evaluate([matchingRule, nonMatchingRule], facts);

        results.Should().HaveCount(2);
        results[0].Matched.Should().BeTrue();
        results[0].TriggeredActions.Should().ContainSingle(a => a.ActionCode == "EVACUATE");
        results[1].Matched.Should().BeFalse();
        results[1].TriggeredActions.Should().BeEmpty();
    }

    [Fact]
    public void EnterpriseRulesEngine_EvaluateByType_FiltersToOnlyThatType()
    {
        var hazardRule = new EnterpriseRule(
            EnterpriseRuleId.New(), "Hazard rule", EnterpriseRuleType.Hazard,
            [new EnterpriseRuleCondition("HazardType", EnterpriseRuleComparisonOperator.Equals, "Fire")],
            [new EnterpriseRuleAction("A1", "d")]);
        var escalationRule = new EnterpriseRule(
            EnterpriseRuleId.New(), "Escalation rule", EnterpriseRuleType.Escalation,
            [new EnterpriseRuleCondition("HazardType", EnterpriseRuleComparisonOperator.Equals, "Fire")],
            [new EnterpriseRuleAction("A2", "d")]);
        var facts = new Dictionary<string, string> { ["HazardType"] = "Fire" };

        IReadOnlyList<EnterpriseRuleEvaluationResult> results = EnterpriseRulesEngine.EvaluateByType([hazardRule, escalationRule], EnterpriseRuleType.Escalation, facts);

        results.Should().ContainSingle().Which.RuleName.Should().Be("Escalation rule");
    }
}

// ============================================================================
//  Project:        Enterprise.RulesEngine
//  Module:         Abstractions.Enums
//  File:           EnterpriseRuleType.cs
//  Created:        2026-08-08
//  Description:    Phase D.5: the vocabulary of rule categories the
//                  enterprise rules engine supports, per Path D's spec.
//                  Additive-only: new members may be appended, existing
//                  members must never be renumbered or removed once
//                  published.
// ============================================================================
namespace Enterprise.RulesEngine.Abstractions.Enums;

/// <summary>The category of an <c>EnterpriseRule</c>.</summary>
public enum EnterpriseRuleType
{
    /// <summary>A rule reacting to hazard classification/occurrence facts.</summary>
    Hazard,

    /// <summary>A rule reacting to severity-level facts.</summary>
    Severity,

    /// <summary>A rule reacting to regulatory-jurisdiction facts.</summary>
    Regulatory,

    /// <summary>A rule reacting to workflow-state facts.</summary>
    Workflow,

    /// <summary>A rule that escalates handling based on other facts (e.g. repeated triggers, chain length).</summary>
    Escalation,
}

/// <summary>The comparison an <c>EnterpriseRuleCondition</c> applies between a fact's value and its comparison value.</summary>
public enum EnterpriseRuleComparisonOperator
{
    /// <summary>The fact value equals the comparison value (ordinal string comparison).</summary>
    Equals,

    /// <summary>The fact value does not equal the comparison value (ordinal string comparison).</summary>
    NotEquals,

    /// <summary>The fact value, parsed as a number, is greater than the comparison value.</summary>
    GreaterThan,

    /// <summary>The fact value, parsed as a number, is less than the comparison value.</summary>
    LessThan,

    /// <summary>The fact value contains the comparison value as a substring (ordinal).</summary>
    Contains,
}

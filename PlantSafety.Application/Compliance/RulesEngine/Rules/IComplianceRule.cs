// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: Contract for a single, small, named, independently testable compliance rule
//   within the Compliance Rules Engine pipeline (Handbook Phase 5 §6.3). Every registered
//   implementation is invoked by ComplianceEvaluator for each evaluation run whose Category
//   matches the rule's own Category.
// SchemaVersion: 1

using PlantSafety.Domain.Compliance.RulesEngine.Enums;

namespace PlantSafety.Application.Compliance.RulesEngine.Rules;

/// <summary>Contract for a single named compliance rule evaluated by the Compliance Rules Engine.</summary>
public interface IComplianceRule
{
    /// <summary>Stable machine-readable rule code (e.g. "Safety.ControlMeasureJustificationRequired").</summary>
    string RuleCode { get; }

    /// <summary>Human-readable rule name.</summary>
    string RuleName { get; }

    /// <summary>Governing category pipeline (Safety, Regulatory, or InternalPolicy) this rule belongs to.</summary>
    ComplianceRuleCategory Category { get; }

    /// <summary>
    /// Deterministically evaluates this rule against <paramref name="context"/> and returns a
    /// pass/fail outcome with violation details. Must not perform I/O; all data the rule needs
    /// must already be present in <paramref name="context"/>.
    /// </summary>
    ComplianceRuleOutcome Evaluate(ComplianceRuleContext context);
}

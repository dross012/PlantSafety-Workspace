// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: Safety-compliance rule verifying that any hazard control measure whose
//   ControlHierarchyLevel requires written justification (per RiskManagement's
//   ControlHierarchyLevel.RequiresJustification) has one recorded. Mirrors the invariant already
//   enforced at the Risk aggregate boundary (RiskControlJustificationRequiredException) so the
//   Compliance Rules Engine can flag any record that slipped through, e.g. via legacy data.
// SchemaVersion: 1

using PlantSafety.Domain.Compliance.RulesEngine.Enums;
using PlantSafety.Application.Compliance.RulesEngine.Rules;

namespace PlantSafety.Application.Compliance.RulesEngine.Rules.Safety;

/// <summary>Verifies that control measures requiring justification have one recorded.</summary>
public sealed class ControlMeasureJustificationComplianceRule : IComplianceRule
{
    /// <inheritdoc/>
    public string RuleCode => "Safety.ControlMeasureJustificationRequired";

    /// <inheritdoc/>
    public string RuleName => "Control Measure Justification Required";

    /// <inheritdoc/>
    public ComplianceRuleCategory Category => ComplianceRuleCategory.Safety;

    /// <inheritdoc/>
    public ComplianceRuleOutcome Evaluate(ComplianceRuleContext context)
    {
        var requiresJustification = context.FactAsBool("ControlMeasure.RequiresJustification");
        if (!requiresJustification)
            return ComplianceRuleOutcome.Pass();

        var justification = context.Fact("ControlMeasure.Justification");
        if (!string.IsNullOrWhiteSpace(justification))
            return ComplianceRuleOutcome.Pass();

        return ComplianceRuleOutcome.Fail(new ComplianceRuleViolationDetail(
            "ControlMeasure.JustificationMissing",
            "A control measure at a hierarchy level requiring justification has none recorded.",
            ComplianceSeverity.High,
            "Record a written justification for the applied control measure before closing the finding."));
    }
}

// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: Internal-policy compliance rule verifying that any change flagged as
//   requiring Management of Change (MoC) review has a recorded MoC approval before it is marked
//   implemented.
// SchemaVersion: 1

using PlantSafety.Domain.Compliance.RulesEngine.Enums;
using PlantSafety.Application.Compliance.RulesEngine.Rules;

namespace PlantSafety.Application.Compliance.RulesEngine.Rules.InternalPolicy;

/// <summary>Verifies that changes requiring Management of Change review have a recorded approval.</summary>
public sealed class ManagementOfChangeApprovalComplianceRule : IComplianceRule
{
    /// <inheritdoc/>
    public string RuleCode => "InternalPolicy.MoCApprovalRequired";

    /// <inheritdoc/>
    public string RuleName => "Management of Change Approval Required";

    /// <inheritdoc/>
    public ComplianceRuleCategory Category => ComplianceRuleCategory.InternalPolicy;

    /// <inheritdoc/>
    public ComplianceRuleOutcome Evaluate(ComplianceRuleContext context)
    {
        var requiresMoc = context.FactAsBool("Change.RequiresManagementOfChange");
        var isImplemented = context.FactAsBool("Change.IsImplemented");

        if (!requiresMoc || !isImplemented)
            return ComplianceRuleOutcome.Pass();

        var mocApproved = context.FactAsBool("Change.ManagementOfChangeApproved");
        if (mocApproved)
            return ComplianceRuleOutcome.Pass();

        return ComplianceRuleOutcome.Fail(new ComplianceRuleViolationDetail(
            "Change.MoCApprovalMissing",
            "Change was implemented without a recorded Management of Change approval.",
            ComplianceSeverity.Critical,
            "Halt further work on the change and route it through Management of Change review retroactively."));
    }
}

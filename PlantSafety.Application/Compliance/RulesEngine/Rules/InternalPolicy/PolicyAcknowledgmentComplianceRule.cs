// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: Internal-policy compliance rule verifying that a policy/procedure requiring
//   acknowledgment has been acknowledged by its assigned owner/responsible party within the
//   mandated acknowledgment window.
// SchemaVersion: 1

using PlantSafety.Domain.Compliance.RulesEngine.Enums;
using PlantSafety.Application.Compliance.RulesEngine.Rules;

namespace PlantSafety.Application.Compliance.RulesEngine.Rules.InternalPolicy;

/// <summary>Verifies that policies requiring acknowledgment have been acknowledged on time.</summary>
public sealed class PolicyAcknowledgmentComplianceRule : IComplianceRule
{
    private const int AcknowledgmentWindowDays = 14;

    /// <inheritdoc/>
    public string RuleCode => "InternalPolicy.AcknowledgmentOverdue";

    /// <inheritdoc/>
    public string RuleName => "Policy Acknowledgment Overdue";

    /// <inheritdoc/>
    public ComplianceRuleCategory Category => ComplianceRuleCategory.InternalPolicy;

    /// <inheritdoc/>
    public ComplianceRuleOutcome Evaluate(ComplianceRuleContext context)
    {
        var requiresAcknowledgment = context.FactAsBool("Policy.RequiresAcknowledgment");
        if (!requiresAcknowledgment)
            return ComplianceRuleOutcome.Pass();

        var publishedAtUtc = context.FactAsDateTime("Policy.PublishedAtUtc");
        if (publishedAtUtc is null)
            return ComplianceRuleOutcome.Pass();

        var acknowledgedAtUtc = context.FactAsDateTime("Policy.AcknowledgedAtUtc");
        if (acknowledgedAtUtc is not null)
            return ComplianceRuleOutcome.Pass();

        var deadline = publishedAtUtc.Value.AddDays(AcknowledgmentWindowDays);
        if (context.EvaluatedAtUtc <= deadline)
            return ComplianceRuleOutcome.Pass();

        return ComplianceRuleOutcome.Fail(new ComplianceRuleViolationDetail(
            "Policy.AcknowledgmentOverdue",
            $"Policy was published {publishedAtUtc:O} and remains unacknowledged past the {AcknowledgmentWindowDays}-day window.",
            ComplianceSeverity.Medium,
            "Obtain and record the responsible party's acknowledgment."));
    }
}

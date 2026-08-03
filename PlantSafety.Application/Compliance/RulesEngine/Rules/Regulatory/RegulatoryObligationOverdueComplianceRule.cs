// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: Regulatory-compliance rule flagging a statutory/regulatory obligation whose
//   DueAtUtc fact has passed while its CompletedAtUtc fact remains unset.
// SchemaVersion: 1

using PlantSafety.Domain.Compliance.RulesEngine.Enums;
using PlantSafety.Application.Compliance.RulesEngine.Rules;

namespace PlantSafety.Application.Compliance.RulesEngine.Rules.Regulatory;

/// <summary>Flags overdue, uncompleted regulatory obligations.</summary>
public sealed class RegulatoryObligationOverdueComplianceRule : IComplianceRule
{
    /// <inheritdoc/>
    public string RuleCode => "Regulatory.ObligationOverdue";

    /// <inheritdoc/>
    public string RuleName => "Regulatory Obligation Overdue";

    /// <inheritdoc/>
    public ComplianceRuleCategory Category => ComplianceRuleCategory.Regulatory;

    /// <inheritdoc/>
    public ComplianceRuleOutcome Evaluate(ComplianceRuleContext context)
    {
        var dueAtUtc = context.FactAsDateTime("RegulatoryObligation.DueAtUtc");
        if (dueAtUtc is null)
            return ComplianceRuleOutcome.Pass();

        var completedAtUtc = context.FactAsDateTime("RegulatoryObligation.CompletedAtUtc");
        if (completedAtUtc is not null)
            return ComplianceRuleOutcome.Pass();

        if (dueAtUtc.Value > context.EvaluatedAtUtc)
            return ComplianceRuleOutcome.Pass();

        var daysOverdue = (context.EvaluatedAtUtc - dueAtUtc.Value).TotalDays;
        var severity = daysOverdue >= 30 ? ComplianceSeverity.Critical : ComplianceSeverity.High;

        return ComplianceRuleOutcome.Fail(new ComplianceRuleViolationDetail(
            "RegulatoryObligation.Overdue",
            $"Regulatory obligation was due {dueAtUtc:O} and remains incomplete ({daysOverdue:F0} day(s) overdue).",
            severity,
            "Complete the regulatory obligation and record the completion date, or escalate to the compliance owner."));
    }
}

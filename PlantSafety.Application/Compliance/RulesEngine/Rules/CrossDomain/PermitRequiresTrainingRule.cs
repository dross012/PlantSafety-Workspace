// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-05
// FILE DESCRIPTION: First cross-domain compliance rule -- combines facts from two different
//   modules (Compliance.Permits and Operational.Training) in a single evaluation, unlike every
//   other IComplianceRule registered today (Safety/Regulatory/InternalPolicy), which each read
//   facts from only one entity type. Detects an open/active permit whose requesting worker has
//   no training record on file at all.
//
//   Deliberately does NOT attempt to match the permit's PermitType against a specific required
//   training course/code: no field on Permits (PermitsItemDto) or anywhere else in this codebase
//   models "this permit type requires this training course" as an explicit relationship, and
//   inventing that mapping (e.g. string-matching "HotWork" against a course name) would be
//   fabricating a cross-domain relationship this codebase does not actually define. This rule
//   checks only what real data supports: does the requesting worker have any training record on
//   file at all.
// SchemaVersion: 1

using PlantSafety.Domain.Compliance.RulesEngine.Enums;

namespace PlantSafety.Application.Compliance.RulesEngine.Rules.CrossDomain;

/// <summary>Detects an open/active permit whose requesting worker has no training record on file.</summary>
public sealed class PermitRequiresTrainingRule : IComplianceRule
{
    private static readonly string[] OpenStatuses = ["Active", "Open", "InProgress", "Approved"];

    /// <inheritdoc/>
    public string RuleCode => "Safety.PermitRequesterHasNoTrainingRecord";

    /// <inheritdoc/>
    public string RuleName => "Permit Requester Has No Training Record";

    /// <inheritdoc/>
    public ComplianceRuleCategory Category => ComplianceRuleCategory.Safety;

    /// <inheritdoc/>
    public ComplianceRuleOutcome Evaluate(ComplianceRuleContext context)
    {
        var status = context.Fact("Permit.Status");
        var hasTrainingRecord = context.FactAsBool("Worker.HasTrainingRecordOnFile", defaultValue: true);

        if (status is null)
            return ComplianceRuleOutcome.Pass();

        var isOpen = OpenStatuses.Contains(status, StringComparer.OrdinalIgnoreCase);

        if (!isOpen || hasTrainingRecord)
            return ComplianceRuleOutcome.Pass();

        return ComplianceRuleOutcome.Fail(new ComplianceRuleViolationDetail(
            "Permit.RequesterHasNoTrainingRecord",
            "The permit's requesting worker has no training record on file.",
            ComplianceSeverity.Critical,
            "Confirm the requesting worker has completed the training required for this permitted work before proceeding."));
    }
}

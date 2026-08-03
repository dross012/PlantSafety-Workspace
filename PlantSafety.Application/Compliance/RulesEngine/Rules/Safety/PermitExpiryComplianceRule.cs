// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: Safety-compliance rule detecting permits (permit-to-work / hot-work /
//   confined-space, etc.) whose ExpiresAtUtc fact has already passed the evaluation instant
//   while the permit's Status fact still shows it as open/active.
// SchemaVersion: 1

using PlantSafety.Domain.Compliance.RulesEngine.Enums;
using PlantSafety.Application.Compliance.RulesEngine.Rules;

namespace PlantSafety.Application.Compliance.RulesEngine.Rules.Safety;

/// <summary>Detects permits that remain open/active past their recorded expiry instant.</summary>
public sealed class PermitExpiryComplianceRule : IComplianceRule
{
    private static readonly string[] OpenStatuses = ["Active", "Open", "InProgress", "Approved"];

    /// <inheritdoc/>
    public string RuleCode => "Safety.PermitExpiredWhileOpen";

    /// <inheritdoc/>
    public string RuleName => "Permit Expired While Open";

    /// <inheritdoc/>
    public ComplianceRuleCategory Category => ComplianceRuleCategory.Safety;

    /// <inheritdoc/>
    public ComplianceRuleOutcome Evaluate(ComplianceRuleContext context)
    {
        var expiresAtUtc = context.FactAsDateTime("Permit.ExpiresAtUtc");
        var status = context.Fact("Permit.Status");

        if (expiresAtUtc is null || status is null)
            return ComplianceRuleOutcome.Pass();

        var isOpen = OpenStatuses.Contains(status, StringComparer.OrdinalIgnoreCase);
        var isExpired = expiresAtUtc.Value <= context.EvaluatedAtUtc;

        if (!isOpen || !isExpired)
            return ComplianceRuleOutcome.Pass();

        return ComplianceRuleOutcome.Fail(new ComplianceRuleViolationDetail(
            "Permit.ExpiredWhileOpen",
            $"Permit expired at {expiresAtUtc:O} but remains in status '{status}'.",
            ComplianceSeverity.Critical,
            "Close, suspend, or re-authorize the permit immediately; do not perform work under an expired permit."));
    }
}

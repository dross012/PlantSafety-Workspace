// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: Regulatory-compliance rule flagging a required certification/credential
//   whose ExpiresAtUtc fact has passed the evaluation instant without a recorded renewal.
// SchemaVersion: 1

using PlantSafety.Domain.Compliance.RulesEngine.Enums;
using PlantSafety.Application.Compliance.RulesEngine.Rules;

namespace PlantSafety.Application.Compliance.RulesEngine.Rules.Regulatory;

/// <summary>Flags expired required certifications.</summary>
public sealed class CertificationExpiredComplianceRule : IComplianceRule
{
    /// <inheritdoc/>
    public string RuleCode => "Regulatory.CertificationExpired";

    /// <inheritdoc/>
    public string RuleName => "Certification Expired";

    /// <inheritdoc/>
    public ComplianceRuleCategory Category => ComplianceRuleCategory.Regulatory;

    /// <inheritdoc/>
    public ComplianceRuleOutcome Evaluate(ComplianceRuleContext context)
    {
        var expiresAtUtc = context.FactAsDateTime("Certification.ExpiresAtUtc");
        if (expiresAtUtc is null || expiresAtUtc.Value > context.EvaluatedAtUtc)
            return ComplianceRuleOutcome.Pass();

        var renewedAtUtc = context.FactAsDateTime("Certification.RenewedAtUtc");
        if (renewedAtUtc is not null && renewedAtUtc.Value > expiresAtUtc.Value)
            return ComplianceRuleOutcome.Pass();

        return ComplianceRuleOutcome.Fail(new ComplianceRuleViolationDetail(
            "Certification.Expired",
            $"Required certification expired at {expiresAtUtc:O} with no recorded renewal.",
            ComplianceSeverity.High,
            "Renew the certification and record the renewal date."));
    }
}

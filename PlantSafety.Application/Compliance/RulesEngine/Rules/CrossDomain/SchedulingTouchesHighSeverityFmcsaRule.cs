// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Cross-domain rule surfacing "this scheduled work touches High/Critical-
//   severity FMCSA requirements" -- an informational flag for Scheduling/Routing, not a
//   compliance violation. Always Advisory severity (see ComplianceSeverity.Advisory: "does not
//   affect overall pass/fail" and ComplianceOutcomePolicy, which never fails an evaluation on
//   Advisory alone) -- exposure to a regulatory area is not itself a violation of anything.
//   Mirrors SchedulingTouchesHighSeverityOshaRule exactly.
//
//   Deliberately does NOT derive which FMCSA module a piece of scheduled work touches from any
//   Scheduling field. Instead, like every other IComplianceRule, this one only reads
//   already-resolved facts the caller supplies: "Fmcsa.Module" (which FMCSA module the caller has
//   determined this work belongs to) and "Fmcsa.HasHighSeverityRequirements" (gathered via
//   FmcsaStandardsFactProvider.GetHighSeverityFmcsaExposureAsync before evaluation).
// SchemaVersion: 1

using PlantSafety.Domain.Compliance.RulesEngine.Enums;

namespace PlantSafety.Application.Compliance.RulesEngine.Rules.CrossDomain;

/// <summary>Flags scheduled work that touches an FMCSA module with High/Critical-severity requirements on file. Informational only.</summary>
public sealed class SchedulingTouchesHighSeverityFmcsaRule : IComplianceRule
{
    /// <inheritdoc/>
    public string RuleCode => "Safety.SchedulingTouchesHighSeverityFmcsa";

    /// <inheritdoc/>
    public string RuleName => "Scheduled Work Touches High-Severity FMCSA Requirements";

    /// <inheritdoc/>
    public ComplianceRuleCategory Category => ComplianceRuleCategory.Safety;

    /// <inheritdoc/>
    public ComplianceRuleOutcome Evaluate(ComplianceRuleContext context)
    {
        var module = context.Fact("Fmcsa.Module");
        var hasHighSeverityRequirements = context.FactAsBool("Fmcsa.HasHighSeverityRequirements");

        if (module is null || !hasHighSeverityRequirements)
            return ComplianceRuleOutcome.Pass();

        return ComplianceRuleOutcome.Fail(new ComplianceRuleViolationDetail(
            "Scheduling.TouchesHighSeverityFmcsa",
            $"This scheduled work touches the '{module}' FMCSA module, which has High or Critical-severity requirements on file.",
            ComplianceSeverity.Advisory,
            $"Review Compliance.FMCSARequirement entries for AppliesToModule = '{module}' with Severity in ('High','Critical') before starting work."));
    }
}

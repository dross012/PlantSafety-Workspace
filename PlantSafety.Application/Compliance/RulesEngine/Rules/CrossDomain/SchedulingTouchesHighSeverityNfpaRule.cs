// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Cross-domain rule surfacing "this scheduled work touches High/Critical-
//   severity NFPA hazard guidance" -- an informational flag for Scheduling, not a compliance
//   violation. NFPA is not a government agency; its codes are voluntary consensus guidance, not
//   legally binding regulations (unlike OSHA's CFR Title 29) -- always Advisory severity, exactly
//   as SchedulingTouchesHighSeverityOshaRule already treats OSHA exposure, but here the framing
//   is doubly non-mandatory: exposure to a hazard area recommended-controls list is neither a
//   violation of anything nor evidence a legal obligation exists.
//
//   Deliberately does NOT derive which NFPA module a piece of scheduled work touches from any
//   Scheduling field, for the same reason SchedulingTouchesHighSeverityOshaRule doesn't. Instead,
//   like every other IComplianceRule, this one only reads already-resolved facts the caller
//   supplies: "Nfpa.Module" (which PlantSafety module the caller has determined this work belongs
//   to) and "Nfpa.HasHighSeverityRequirements" (gathered via
//   NfpaReferenceFactProvider.GetHighSeverityNfpaExposureAsync before evaluation).
// SchemaVersion: 1

using PlantSafety.Domain.Compliance.RulesEngine.Enums;

namespace PlantSafety.Application.Compliance.RulesEngine.Rules.CrossDomain;

/// <summary>Flags scheduled work that touches an NFPA module with High/Critical-severity hazard mappings on file. Informational only.</summary>
public sealed class SchedulingTouchesHighSeverityNfpaRule : IComplianceRule
{
    /// <inheritdoc/>
    public string RuleCode => "Safety.SchedulingTouchesHighSeverityNfpa";

    /// <inheritdoc/>
    public string RuleName => "Scheduled Work Touches High-Severity NFPA Hazard Guidance";

    /// <inheritdoc/>
    public ComplianceRuleCategory Category => ComplianceRuleCategory.Safety;

    /// <inheritdoc/>
    public ComplianceRuleOutcome Evaluate(ComplianceRuleContext context)
    {
        var module = context.Fact("Nfpa.Module");
        var hasHighSeverityRequirements = context.FactAsBool("Nfpa.HasHighSeverityRequirements");

        if (module is null || !hasHighSeverityRequirements)
            return ComplianceRuleOutcome.Pass();

        return ComplianceRuleOutcome.Fail(new ComplianceRuleViolationDetail(
            "Scheduling.TouchesHighSeverityNfpa",
            $"This scheduled work touches the '{module}' PlantSafety module, which has High or Critical-severity NFPA hazard guidance on file.",
            ComplianceSeverity.Advisory,
            $"Review Compliance.NFPA_HazardModuleMapping entries for ModuleName = '{module}' with Severity in ('High','Critical') before starting work."));
    }
}

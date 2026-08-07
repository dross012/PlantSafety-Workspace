// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Cross-domain rule surfacing "this scheduled work touches High/Critical-
//   severity OSHA requirements" -- an informational flag for Scheduling (PMScheduling,
//   HazardIsolationSequencing, WorkOrderPriority, etc.), not a compliance violation. Always
//   Advisory severity (see ComplianceSeverity.Advisory: "does not affect overall pass/fail" and
//   ComplianceOutcomePolicy, which never fails an evaluation on Advisory alone) -- exposure to a
//   regulatory area is not itself a violation of anything.
//
//   Deliberately does NOT derive which OSHA module a piece of scheduled work touches from any
//   Scheduling field. No Scheduling aggregate (PMScheduling.PmTaskType, WorkOrderPriority's
//   free-text HazardDescription, etc.) encodes a PlantSafety-module vocabulary, and guessing
//   from free text would fabricate a relationship this codebase doesn't define (the same
//   restraint PermitRequiresTrainingRule already applies to PermitType->training-course
//   matching). Instead, like every other IComplianceRule, this one only reads
//   already-resolved facts the caller supplies: "Osha.Module" (which OSHA module the caller has
//   determined this work belongs to -- e.g. "MachineSafety" for PMScheduling/
//   HazardIsolationSequencing, whose entire domain purpose is equipment/hazardous-energy
//   control, the same real-world work CFR 1910.147 governs) and
//   "Osha.HasHighSeverityRequirements" (gathered via
//   OshaStandardsFactProvider.GetHighSeverityExposureAsync before evaluation).
// SchemaVersion: 1

using PlantSafety.Domain.Compliance.RulesEngine.Enums;

namespace PlantSafety.Application.Compliance.RulesEngine.Rules.CrossDomain;

/// <summary>Flags scheduled work that touches an OSHA module with High/Critical-severity requirements on file. Informational only.</summary>
public sealed class SchedulingTouchesHighSeverityOshaRule : IComplianceRule
{
    /// <inheritdoc/>
    public string RuleCode => "Safety.SchedulingTouchesHighSeverityOsha";

    /// <inheritdoc/>
    public string RuleName => "Scheduled Work Touches High-Severity OSHA Requirements";

    /// <inheritdoc/>
    public ComplianceRuleCategory Category => ComplianceRuleCategory.Safety;

    /// <inheritdoc/>
    public ComplianceRuleOutcome Evaluate(ComplianceRuleContext context)
    {
        var module = context.Fact("Osha.Module");
        var hasHighSeverityRequirements = context.FactAsBool("Osha.HasHighSeverityRequirements");

        if (module is null || !hasHighSeverityRequirements)
            return ComplianceRuleOutcome.Pass();

        return ComplianceRuleOutcome.Fail(new ComplianceRuleViolationDetail(
            "Scheduling.TouchesHighSeverityOsha",
            $"This scheduled work touches the '{module}' OSHA module, which has High or Critical-severity requirements on file.",
            ComplianceSeverity.Advisory,
            $"Review Compliance.OSHARequirement entries for AppliesToModule = '{module}' with Severity in ('High','Critical') before starting work."));
    }
}

// ============================================================================
//  Project:        AI.SeverityAssessment
//  Module:         Abstractions.Rules
//  File:           SeverityAssessmentRuleSet.cs
//  Created:        2026-08-09
//  Description:    The standard, deterministic rule set for
//                  SeverityAssessmentEngine.Assess -- explanatory notes
//                  only (never overrides BaseSeverity/FinalAssessedSeverity,
//                  which stay derived from Shared.FacilityDomain's real
//                  SeverityInferenceRules per Phase E.5's composition rule).
// ============================================================================
using Shared.FacilityDomain.Abstractions.Hazards;
using Shared.FacilityDomain.Abstractions.Severity;

namespace AI.SeverityAssessment.Abstractions.Rules;

/// <summary>The standard rule set applied to every severity assessment.</summary>
public static class SeverityAssessmentRuleSet
{
    /// <summary>The standard, deterministic rules.</summary>
    public static IReadOnlyList<SeverityAssessmentRule> Standard { get; } =
    [
        new SeverityAssessmentRule(
            "SEV-001",
            "Multiple injuries",
            (input, _) => input.EmployeeInjuryCount > 1,
            "Multiple employee injuries reported; verify all affected employees have been accounted for."),

        new SeverityAssessmentRule(
            "SEV-002",
            "Cascading machine failures",
            (input, _) => input.MachineFailureCount >= 2,
            "Two or more machine failures indicate a possible cascading equipment failure, not an isolated fault."),

        new SeverityAssessmentRule(
            "SEV-003",
            "Long propagation chain",
            (input, _) => input.EventChainLength >= 3,
            "Event chain length of 3 or more indicates the hazard has propagated through multiple stages."),

        new SeverityAssessmentRule(
            "SEV-004",
            "Digital twin disagrees with base assessment",
            (input, baseSeverity) => input.DigitalTwinSimulatedSeverity != baseSeverity,
            "The digital-twin simulated severity differs from the base inferred severity; the worst-case value was used."),

        new SeverityAssessmentRule(
            "SEV-005",
            "Fire with any injury",
            (input, _) => input.HazardType == HazardType.Fire && input.EmployeeInjuryCount > 0,
            "Fire hazard with employee injury; confirm evacuation and fire-suppression status."),

        new SeverityAssessmentRule(
            "SEV-006",
            "Catastrophic digital twin projection",
            (input, _) => input.DigitalTwinSimulatedSeverity == SeverityLevel.Catastrophic,
            "Digital twin projects a catastrophic outcome if current conditions continue unmitigated."),
    ];
}

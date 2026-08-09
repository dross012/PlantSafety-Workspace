// ============================================================================
//  Project:        AI.SeverityAssessment
//  Module:         Abstractions.Engine
//  File:           SeverityAssessmentEngine.cs
//  Created:        2026-08-08
//  Description:    Phase E.5 (Path E: AI-Driven Safety Automation):
//                  assesses severity from hazard context, employee
//                  injury, machine failure, and hazard chains by calling
//                  Shared.FacilityDomain's real SeverityInferenceRules.Infer
//                  (Phase C.3, unchanged), then folds in digital-twin
//                  simulated severity (Phase D.7) via worst-case
//                  comparison. Pure function, no side effects, no
//                  reflection.
// ============================================================================
using Shared.FacilityDomain.Abstractions.Severity;

using AI.SeverityAssessment.Abstractions.Assessment;
using AI.SeverityAssessment.Abstractions.Rules;

namespace AI.SeverityAssessment.Abstractions.Engine;

/// <summary>Deterministically assesses severity across hazard, injury, failure, chain, and digital-twin signals.</summary>
public static class SeverityAssessmentEngine
{
    /// <summary>Assesses severity for the given input and returns the aggregated assessment output.</summary>
    public static SeverityAssessmentOutput Assess(SeverityAssessmentInput input, IReadOnlyList<SeverityAssessmentRule> rules, DateTime? nowUtc = null)
    {
        SeverityLevel baseSeverity = SeverityInferenceRules.Infer(input.HazardType, input.EmployeeInjuryCount, input.MachineFailureCount, input.EventChainLength);
        SeverityLevel finalSeverity = baseSeverity >= input.DigitalTwinSimulatedSeverity ? baseSeverity : input.DigitalTwinSimulatedSeverity;

        IReadOnlyList<string> notes = rules
            .Where(rule => rule.Condition(input, baseSeverity))
            .Select(rule => rule.Note)
            .ToArray();

        return new SeverityAssessmentOutput(baseSeverity, input.DigitalTwinSimulatedSeverity, finalSeverity, notes, nowUtc ?? DateTime.UtcNow);
    }
}

// ============================================================================
//  Project:        AI.SeverityAssessment
//  Module:         Abstractions.Assessment
//  File:           SeverityAssessmentInput.cs
//  Created:        2026-08-08
//  Description:    Phase E.5 (Path E: AI-Driven Safety Automation): the
//                  input SeverityAssessmentEngine.Assess reasons over --
//                  hazard context, employee injury, machine failure, and
//                  event-chain length feed Shared.FacilityDomain's real
//                  SeverityInferenceRules (Phase C.3) directly;
//                  DigitalTwinSimulatedSeverity is the fifth signal Path
//                  E's spec requires (a simulated outcome from Enterprise.
//                  DigitalTwin, Phase D.7), compared against the inferred
//                  severity for the final worst-case assessment.
// ============================================================================
using Shared.FacilityDomain.Abstractions.Hazards;
using Shared.FacilityDomain.Abstractions.Severity;

namespace AI.SeverityAssessment.Abstractions.Assessment;

/// <summary>The input a severity-assessment session reasons over.</summary>
public sealed record SeverityAssessmentInput(
    HazardType HazardType,
    int EmployeeInjuryCount,
    int MachineFailureCount,
    int EventChainLength,
    SeverityLevel DigitalTwinSimulatedSeverity);

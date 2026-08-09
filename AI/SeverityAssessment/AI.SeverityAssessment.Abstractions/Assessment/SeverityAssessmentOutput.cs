// ============================================================================
//  Project:        AI.SeverityAssessment
//  Module:         Abstractions.Assessment
//  File:           SeverityAssessmentOutput.cs
//  Created:        2026-08-08
//  Description:    Phase E.5: the result of a severity-assessment session
//                  -- the base inferred severity, the digital-twin
//                  simulated severity, the worst-case final assessment,
//                  and any notes contributed by matched SeverityAssessmentRules.
// ============================================================================
using Shared.FacilityDomain.Abstractions.Severity;

namespace AI.SeverityAssessment.Abstractions.Assessment;

/// <summary>The result of a severity-assessment session.</summary>
public sealed record SeverityAssessmentOutput(
    SeverityLevel BaseSeverity,
    SeverityLevel DigitalTwinSimulatedSeverity,
    SeverityLevel FinalAssessedSeverity,
    IReadOnlyList<string> Notes,
    DateTime GeneratedAtUtc);

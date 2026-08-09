// ============================================================================
//  Project:        AI.SeverityAssessment
//  Module:         Abstractions.Rules
//  File:           SeverityAssessmentRule.cs
//  Created:        2026-08-08
//  Description:    Phase E.5: a single deterministic rule contributing an
//                  explanatory note to a severity assessment. Condition
//                  receives both the raw input and the base severity
//                  already inferred by Shared.FacilityDomain's
//                  SeverityInferenceRules, so rules can react to either
//                  (or their combination) without re-deriving severity
//                  themselves.
// ============================================================================
using Shared.FacilityDomain.Abstractions.Severity;

using AI.SeverityAssessment.Abstractions.Assessment;

namespace AI.SeverityAssessment.Abstractions.Rules;

/// <summary>A single deterministic rule contributing an explanatory note to a severity assessment.</summary>
public sealed record SeverityAssessmentRule(
    string RuleId,
    string Name,
    Func<SeverityAssessmentInput, SeverityLevel, bool> Condition,
    string Note);

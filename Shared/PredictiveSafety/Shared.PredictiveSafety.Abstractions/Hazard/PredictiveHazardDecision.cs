// ============================================================================
//  Project:        Shared.PredictiveSafety.Abstractions
//  Module:         Hazard
//  File:           PredictiveHazardDecision.cs
//  Created:        2026-08-10
//  Description:    Phase 15 deliverable. The deterministic output of a
//                  hazard prediction: PredictedHazardType/PredictedSeverity/
//                  PredictedJurisdictions come unchanged from Facility's
//                  real Phase 4 inference rules (HazardInferenceRules,
//                  SeverityInferenceRules, RegulatoryInferenceRules);
//                  RequiresImmediateEscalation/Priority/AppliedRuleCodes come
//                  from PredictiveHazardRuleEngine's cross-module escalation
//                  evaluation.
// ============================================================================
using Shared.FacilityDomain.Abstractions.Hazards;
using Shared.FacilityDomain.Abstractions.Regulatory;
using Shared.FacilityDomain.Abstractions.Severity;
using Shared.PredictiveSafety.Abstractions.Common;

namespace Shared.PredictiveSafety.Abstractions.Hazard;

/// <summary>The deterministic output of a hazard prediction.</summary>
public sealed record PredictiveHazardDecision(
    HazardType PredictedHazardType,
    SeverityLevel PredictedSeverity,
    IReadOnlyList<RegulatoryJurisdiction> PredictedJurisdictions,
    bool RequiresImmediateEscalation,
    PredictivePriority Priority,
    IReadOnlyList<string> AppliedRuleCodes);

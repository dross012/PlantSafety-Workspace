// ============================================================================
//  Project:        Shared.PredictiveSafety.Abstractions
//  Module:         Hazard
//  File:           PredictiveHazardInferenceEngine.cs
//  Created:        2026-08-10
//  Description:    Phase 15 deliverable. The public, pure entry point for a
//                  hazard prediction: reuses Facility's three existing,
//                  unchanged Phase 4 inference rules -- HazardInferenceRules.
//                  InferHazardTypeFromSafetyEvent, SeverityInferenceRules.
//                  Infer, RegulatoryInferenceRules.Infer -- to determine WHAT
//                  is predicted, then calls PredictiveHazardRuleEngine.Evaluate
//                  to determine whether cross-module signals (Routing,
//                  Scheduling, reliability) require immediate escalation.
//                  No new hazard/severity/regulatory inference logic is
//                  invented here -- this is composition, not reimplementation.
// ============================================================================
using Shared.FacilityDomain.Abstractions.Hazards;
using Shared.FacilityDomain.Abstractions.Regulatory;
using Shared.FacilityDomain.Abstractions.Severity;
using Shared.PredictiveSafety.Abstractions.Common;

namespace Shared.PredictiveSafety.Abstractions.Hazard;

/// <summary>Deterministically predicts a hazard by composing Facility's real inference rules with cross-module escalation evaluation.</summary>
public static class PredictiveHazardInferenceEngine
{
    /// <summary>
    /// Infers a HazardType/SeverityLevel/RegulatoryJurisdiction set for <paramref name="signals"/> via
    /// Facility's real inference rules, then evaluates escalation via PredictiveHazardRuleEngine.
    /// Same input always produces the same output.
    /// </summary>
    public static PredictiveHazardDecision Infer(PredictiveHazardSignals signals)
    {
        ArgumentNullException.ThrowIfNull(signals);

        HazardType predictedHazardType = HazardInferenceRules.InferHazardTypeFromSafetyEvent(signals.EventType, signals.Description);

        SeverityLevel predictedSeverity = SeverityInferenceRules.Infer(
            predictedHazardType,
            signals.EmployeeExposureCount,
            signals.MachineFailureCount,
            signals.EventChainLength);

        IReadOnlyList<RegulatoryJurisdiction> predictedJurisdictions = RegulatoryInferenceRules.Infer(predictedHazardType, predictedSeverity);

        PredictiveEscalationOutcome escalation = PredictiveHazardRuleEngine.Evaluate(signals, predictedSeverity);
        PredictivePriority priority = PredictiveEscalationClassifier.Classify(escalation);

        return new PredictiveHazardDecision(
            PredictedHazardType: predictedHazardType,
            PredictedSeverity: predictedSeverity,
            PredictedJurisdictions: predictedJurisdictions,
            RequiresImmediateEscalation: escalation.RequiresEscalation,
            Priority: priority,
            AppliedRuleCodes: escalation.AppliedRuleCodes);
    }
}

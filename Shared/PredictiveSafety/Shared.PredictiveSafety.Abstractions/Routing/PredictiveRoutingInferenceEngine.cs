// ============================================================================
//  Project:        Shared.PredictiveSafety.Abstractions
//  Module:         Routing
//  File:           PredictiveRoutingInferenceEngine.cs
//  Created:        2026-08-10
//  Description:    Phase 15 deliverable. The public, pure entry point for a
//                  routing prediction: calls PredictiveRoutingRuleEngine.
//                  Evaluate, then deterministically classifies the result via
//                  PredictiveEscalationClassifier.
// ============================================================================
using Shared.PredictiveSafety.Abstractions.Common;

namespace Shared.PredictiveSafety.Abstractions.Routing;

/// <summary>Deterministically predicts whether an event requires priority routing from PredictiveRoutingSignals.</summary>
public static class PredictiveRoutingInferenceEngine
{
    /// <summary>Infers a PredictiveRoutingDecision for <paramref name="signals"/>. Same input always produces the same output.</summary>
    public static PredictiveRoutingDecision Infer(PredictiveRoutingSignals signals)
    {
        ArgumentNullException.ThrowIfNull(signals);

        PredictiveEscalationOutcome escalation = PredictiveRoutingRuleEngine.Evaluate(signals);
        PredictivePriority priority = PredictiveEscalationClassifier.Classify(escalation);

        return new PredictiveRoutingDecision(
            EventCode: signals.EventCode,
            FacilityId: signals.FacilityId,
            RequiresPriorityRouting: escalation.RequiresEscalation,
            Priority: priority,
            AppliedRuleCodes: escalation.AppliedRuleCodes);
    }
}

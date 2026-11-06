// ============================================================================
//  Project:        Shared.PredictiveSafety.Abstractions
//  Module:         Scheduling
//  File:           PredictiveSchedulingInferenceEngine.cs
//  Created:        2026-08-10
//  Description:    Phase 15 deliverable. The public, pure entry point for a
//                  scheduling prediction: calls PredictiveSchedulingRuleEngine.
//                  Evaluate, then deterministically classifies the result via
//                  PredictiveEscalationClassifier.
// ============================================================================
using Shared.PredictiveSafety.Abstractions.Common;

namespace Shared.PredictiveSafety.Abstractions.Scheduling;

/// <summary>Deterministically predicts whether scheduling needs to be accelerated from PredictiveSchedulingSignals.</summary>
public static class PredictiveSchedulingInferenceEngine
{
    /// <summary>Infers a PredictiveSchedulingDecision for <paramref name="signals"/>. Same input always produces the same output.</summary>
    public static PredictiveSchedulingDecision Infer(PredictiveSchedulingSignals signals)
    {
        ArgumentNullException.ThrowIfNull(signals);

        PredictiveEscalationOutcome escalation = PredictiveSchedulingRuleEngine.Evaluate(signals);
        PredictivePriority priority = PredictiveEscalationClassifier.Classify(escalation);

        return new PredictiveSchedulingDecision(
            RequiresAcceleratedScheduling: escalation.RequiresEscalation,
            Priority: priority,
            AppliedRuleCodes: escalation.AppliedRuleCodes);
    }
}

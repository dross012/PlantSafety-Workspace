// ============================================================================
//  Project:        Shared.PredictiveSafety.Abstractions
//  Module:         Audit
//  File:           PredictiveAuditInferenceEngine.cs
//  Created:        2026-08-10
//  Description:    Phase 15 deliverable. The public, pure entry point for a
//                  predictive audit evaluation: calls PredictiveAuditRuleEngine.
//                  Evaluate, then deterministically classifies the result via
//                  PredictiveEscalationClassifier.
// ============================================================================
using Shared.PredictiveSafety.Abstractions.Common;

namespace Shared.PredictiveSafety.Abstractions.Audit;

/// <summary>Deterministically evaluates PredictiveAuditSignals for a fixed-threshold audit anomaly.</summary>
public static class PredictiveAuditInferenceEngine
{
    /// <summary>Infers a PredictiveAuditAnomaly for <paramref name="signals"/>. Same input always produces the same output.</summary>
    public static PredictiveAuditAnomaly Infer(PredictiveAuditSignals signals)
    {
        ArgumentNullException.ThrowIfNull(signals);

        PredictiveEscalationOutcome escalation = PredictiveAuditRuleEngine.Evaluate(signals);
        PredictivePriority priority = PredictiveEscalationClassifier.Classify(escalation);

        return new PredictiveAuditAnomaly(
            AnomalyDetected: escalation.RequiresEscalation,
            Priority: priority,
            AppliedRuleCodes: escalation.AppliedRuleCodes);
    }
}

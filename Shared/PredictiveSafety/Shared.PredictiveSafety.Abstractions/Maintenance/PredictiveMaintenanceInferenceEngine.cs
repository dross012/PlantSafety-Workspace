// ============================================================================
//  Project:        Shared.PredictiveSafety.Abstractions
//  Module:         Maintenance
//  File:           PredictiveMaintenanceInferenceEngine.cs
//  Created:        2026-08-10
//  Description:    Phase 15 deliverable. The public, pure entry point for a
//                  maintenance prediction: calls PredictiveMaintenanceRuleEngine.
//                  Evaluate, then deterministically classifies the result via
//                  PredictiveEscalationClassifier.
// ============================================================================
using Shared.PredictiveSafety.Abstractions.Common;

namespace Shared.PredictiveSafety.Abstractions.Maintenance;

/// <summary>Deterministically predicts a machine's maintenance need from PredictiveMaintenanceSignals.</summary>
public static class PredictiveMaintenanceInferenceEngine
{
    /// <summary>Infers a PredictiveMaintenanceDecision for <paramref name="signals"/>. Same input always produces the same output.</summary>
    public static PredictiveMaintenanceDecision Infer(PredictiveMaintenanceSignals signals)
    {
        ArgumentNullException.ThrowIfNull(signals);

        PredictiveEscalationOutcome escalation = PredictiveMaintenanceRuleEngine.Evaluate(signals);
        PredictivePriority priority = PredictiveEscalationClassifier.Classify(escalation);

        return new PredictiveMaintenanceDecision(
            MachineId: signals.MachineId,
            RequiresImmediateMaintenance: escalation.RequiresEscalation,
            Priority: priority,
            AppliedRuleCodes: escalation.AppliedRuleCodes);
    }
}

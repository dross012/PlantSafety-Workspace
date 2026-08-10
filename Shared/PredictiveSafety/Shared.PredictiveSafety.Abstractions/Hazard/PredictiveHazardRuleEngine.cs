// ============================================================================
//  Project:        Shared.PredictiveSafety.Abstractions
//  Module:         Hazard
//  File:           PredictiveHazardRuleEngine.cs
//  Created:        2026-08-10
//  Description:    Phase 15 deliverable. Deterministic, pure escalation
//                  evaluation over PredictiveHazardSignals plus an
//                  already-predicted SeverityLevel (supplied by
//                  PredictiveHazardInferenceEngine, which calls Facility's
//                  real SeverityInferenceRules.Infer first). Additive
//                  accumulation and worst-case-wins only -- no rule ever
//                  clears a prior rule's escalation, no weighted scoring, no
//                  ML.
// ============================================================================
using Shared.FacilityDomain.Abstractions.Severity;
using Shared.PredictiveSafety.Abstractions.Common;

namespace Shared.PredictiveSafety.Abstractions.Hazard;

/// <summary>Deterministic, pure escalation evaluation for a hazard prediction.</summary>
public static class PredictiveHazardRuleEngine
{
    /// <summary>Evaluates <paramref name="signals"/> and <paramref name="predictedSeverity"/> deterministically. Same input always produces the same output.</summary>
    public static PredictiveEscalationOutcome Evaluate(PredictiveHazardSignals signals, SeverityLevel predictedSeverity)
    {
        ArgumentNullException.ThrowIfNull(signals);

        var appliedRuleCodes = new List<string>();

        if (predictedSeverity >= SeverityLevel.Severe)
        {
            appliedRuleCodes.Add("PREDICTED-SEVERITY-ESCALATION");
        }

        if (signals.RoutingTriggeredAuditCount > 0)
        {
            appliedRuleCodes.Add("ROUTING-AUDIT-TRIGGER-ESCALATION");
        }

        if (signals.SchedulingRequiresImmediateAction)
        {
            appliedRuleCodes.Add("SCHEDULING-IMMEDIATE-ACTION-ESCALATION");
        }

        if (signals.RecentReliabilityFailureCount >= 3)
        {
            appliedRuleCodes.Add("RELIABILITY-SIGNAL-ESCALATION");
        }

        return new PredictiveEscalationOutcome(appliedRuleCodes.Count > 0, appliedRuleCodes);
    }
}

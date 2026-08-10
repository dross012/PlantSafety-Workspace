// ============================================================================
//  Project:        Shared.PredictiveSafety.Abstractions
//  Module:         Scheduling
//  File:           PredictiveSchedulingRuleEngine.cs
//  Created:        2026-08-10
//  Description:    Phase 15 deliverable. Deterministic, pure escalation
//                  evaluation over PredictiveSchedulingSignals. Additive
//                  accumulation and worst-case-wins only.
// ============================================================================
using Shared.PredictiveSafety.Abstractions.Common;

namespace Shared.PredictiveSafety.Abstractions.Scheduling;

/// <summary>Deterministic, pure escalation evaluation for a scheduling prediction.</summary>
public static class PredictiveSchedulingRuleEngine
{
    /// <summary>Evaluates <paramref name="signals"/> deterministically. Same input always produces the same output.</summary>
    public static PredictiveEscalationOutcome Evaluate(PredictiveSchedulingSignals signals)
    {
        ArgumentNullException.ThrowIfNull(signals);

        var appliedRuleCodes = new List<string>();

        if (signals.WorstCaseDueWithinHours <= 24)
        {
            appliedRuleCodes.Add("NEAR-TERM-DUE-ESCALATION");
        }

        if (signals.TotalRequiredActionCount >= 3)
        {
            appliedRuleCodes.Add("ACTION-VOLUME-ESCALATION");
        }

        if (signals.HazardPredictionRequiresEscalation)
        {
            appliedRuleCodes.Add("HAZARD-PREDICTION-ESCALATION");
        }

        if (signals.MaintenancePredictionRequiresImmediate)
        {
            appliedRuleCodes.Add("MAINTENANCE-PREDICTION-ESCALATION");
        }

        if (signals.RoutingPredictionRequiresPriority)
        {
            appliedRuleCodes.Add("ROUTING-PREDICTION-ESCALATION");
        }

        return new PredictiveEscalationOutcome(appliedRuleCodes.Count > 0, appliedRuleCodes);
    }
}

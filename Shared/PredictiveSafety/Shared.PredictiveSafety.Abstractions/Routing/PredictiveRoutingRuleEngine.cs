// ============================================================================
//  Project:        Shared.PredictiveSafety.Abstractions
//  Module:         Routing
//  File:           PredictiveRoutingRuleEngine.cs
//  Created:        2026-08-10
//  Description:    Phase 15 deliverable. Deterministic, pure escalation
//                  evaluation over PredictiveRoutingSignals. Additive
//                  accumulation and worst-case-wins only.
// ============================================================================
using Shared.PredictiveSafety.Abstractions.Common;

namespace Shared.PredictiveSafety.Abstractions.Routing;

/// <summary>Deterministic, pure escalation evaluation for a routing prediction.</summary>
public static class PredictiveRoutingRuleEngine
{
    /// <summary>Evaluates <paramref name="signals"/> deterministically. Same input always produces the same output.</summary>
    public static PredictiveEscalationOutcome Evaluate(PredictiveRoutingSignals signals)
    {
        ArgumentNullException.ThrowIfNull(signals);

        var appliedRuleCodes = new List<string>();

        if (signals.TriggeredAuditCount > 0)
        {
            appliedRuleCodes.Add("AUDIT-TRIGGER-ESCALATION");
        }

        if (signals.ChangedComplianceElementCount >= 2)
        {
            appliedRuleCodes.Add("COMPLIANCE-GAP-ESCALATION");
        }

        if (signals.HazardPredictionRequiresEscalation)
        {
            appliedRuleCodes.Add("HAZARD-PREDICTION-ESCALATION");
        }

        if (signals.MaintenancePredictionRequiresImmediate)
        {
            appliedRuleCodes.Add("MAINTENANCE-PREDICTION-ESCALATION");
        }

        return new PredictiveEscalationOutcome(appliedRuleCodes.Count > 0, appliedRuleCodes);
    }
}

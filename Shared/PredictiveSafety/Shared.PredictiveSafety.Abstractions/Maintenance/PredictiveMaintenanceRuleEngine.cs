// ============================================================================
//  Project:        Shared.PredictiveSafety.Abstractions
//  Module:         Maintenance
//  File:           PredictiveMaintenanceRuleEngine.cs
//  Created:        2026-08-10
//  Description:    Phase 15 deliverable. Deterministic, pure escalation
//                  evaluation over PredictiveMaintenanceSignals. Additive
//                  accumulation and worst-case-wins only.
// ============================================================================
using Shared.PredictiveSafety.Abstractions.Common;

namespace Shared.PredictiveSafety.Abstractions.Maintenance;

/// <summary>Deterministic, pure escalation evaluation for a maintenance prediction.</summary>
public static class PredictiveMaintenanceRuleEngine
{
    /// <summary>Evaluates <paramref name="signals"/> deterministically. Same input always produces the same output.</summary>
    public static PredictiveEscalationOutcome Evaluate(PredictiveMaintenanceSignals signals)
    {
        ArgumentNullException.ThrowIfNull(signals);

        var appliedRuleCodes = new List<string>();

        if (signals.IsMaintenanceOverdue)
        {
            appliedRuleCodes.Add("MAINTENANCE-OVERDUE-ESCALATION");
        }

        if (signals.RecentFailureCount >= 2)
        {
            appliedRuleCodes.Add("REPEATED-FAILURE-ESCALATION");
        }

        if (signals.SchedulingWorstCaseDueWithinHours <= 24)
        {
            appliedRuleCodes.Add("SCHEDULING-DUE-SOON-ESCALATION");
        }

        if (signals.RecentReliabilityFailureCount >= 3)
        {
            appliedRuleCodes.Add("RELIABILITY-SIGNAL-ESCALATION");
        }

        return new PredictiveEscalationOutcome(appliedRuleCodes.Count > 0, appliedRuleCodes);
    }
}

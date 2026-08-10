// ============================================================================
//  Project:        Shared.PredictiveSafety.Abstractions
//  Module:         Audit
//  File:           PredictiveAuditRuleEngine.cs
//  Created:        2026-08-10
//  Description:    Phase 15 deliverable. Deterministic, pure fixed-threshold
//                  count evaluation over PredictiveAuditSignals -- additive
//                  accumulation and worst-case-wins only, never a statistical
//                  measure (no mean, no standard deviation, no variance).
// ============================================================================
using Shared.PredictiveSafety.Abstractions.Common;

namespace Shared.PredictiveSafety.Abstractions.Audit;

/// <summary>Deterministic, pure fixed-threshold evaluation for a predictive audit anomaly.</summary>
public static class PredictiveAuditRuleEngine
{
    /// <summary>Evaluates <paramref name="signals"/> deterministically. Same input always produces the same output.</summary>
    public static PredictiveEscalationOutcome Evaluate(PredictiveAuditSignals signals)
    {
        ArgumentNullException.ThrowIfNull(signals);

        var appliedRuleCodes = new List<string>();

        if (signals.WarningOrAboveLogEventCount >= 5)
        {
            appliedRuleCodes.Add("ELEVATED-WARNING-VOLUME");
        }

        if (signals.GovernanceViolationCount > 0)
        {
            appliedRuleCodes.Add("GOVERNANCE-VIOLATION-PRESENT");
        }

        if (signals.DistinctEventIdCount == 1 && signals.TotalLogEventCount >= 10)
        {
            appliedRuleCodes.Add("REPETITIVE-EVENT-PATTERN");
        }

        return new PredictiveEscalationOutcome(appliedRuleCodes.Count > 0, appliedRuleCodes);
    }
}

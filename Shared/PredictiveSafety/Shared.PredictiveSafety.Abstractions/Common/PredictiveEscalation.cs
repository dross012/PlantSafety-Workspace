// ============================================================================
//  Project:        Shared.PredictiveSafety.Abstractions
//  Module:         Common
//  File:           PredictiveEscalation.cs
//  Created:        2026-08-10
//  Description:    Phase 15 deliverable. PredictiveEscalationOutcome is the
//                  shared, additive-accumulation result shape every domain's
//                  RuleEngine.Evaluate returns: a boolean escalation flag and
//                  the fixed-order list of rule codes that produced it --
//                  worst-case-wins (any one applied rule sets the flag; no
//                  rule ever clears it once set), never a weighted score.
//                  PredictiveEscalationClassifier.Classify maps
//                  AppliedRuleCodes.Count to a PredictivePriority via a fixed
//                  threshold table -- deterministic counting, not scoring or
//                  averaging.
// ============================================================================
namespace Shared.PredictiveSafety.Abstractions.Common;

/// <summary>The additive-accumulation, worst-case-wins outcome of a single domain's rule evaluation.</summary>
public sealed record PredictiveEscalationOutcome(bool RequiresEscalation, IReadOnlyList<string> AppliedRuleCodes);

/// <summary>Deterministically classifies an applied-rule count into a PredictivePriority via a fixed threshold table.</summary>
public static class PredictiveEscalationClassifier
{
    /// <summary>Classifies <paramref name="outcome"/> into a PredictivePriority: 0 rules -> Low, 1 -> Medium, 2 -> High, 3+ -> Critical.</summary>
    public static PredictivePriority Classify(PredictiveEscalationOutcome outcome)
    {
        ArgumentNullException.ThrowIfNull(outcome);

        return outcome.AppliedRuleCodes.Count switch
        {
            0 => PredictivePriority.Low,
            1 => PredictivePriority.Medium,
            2 => PredictivePriority.High,
            _ => PredictivePriority.Critical,
        };
    }
}

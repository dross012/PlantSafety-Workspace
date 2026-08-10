// ============================================================================
//  Project:        Shared.PredictiveSafety.Abstractions
//  Module:         Common
//  File:           PredictivePriority.cs
//  Created:        2026-08-10
//  Description:    Phase 15 (Predictive Safety Engine) deliverable. The
//                  deterministic priority vocabulary every prediction domain
//                  (Hazard, Maintenance, Routing, Scheduling, Audit)
//                  classifies its outcome into, via PredictiveEscalationClassifier.
//                  Classification is a fixed threshold on an applied-rule
//                  count -- never a weighted score, never an average, never
//                  probabilistic.
// ============================================================================
namespace Shared.PredictiveSafety.Abstractions.Common;

/// <summary>The deterministic priority a predictive evaluation is classified into, ordered from least to most urgent.</summary>
public enum PredictivePriority
{
    /// <summary>No escalation rule applied.</summary>
    Low,

    /// <summary>Exactly one escalation rule applied.</summary>
    Medium,

    /// <summary>Exactly two escalation rules applied.</summary>
    High,

    /// <summary>Three or more escalation rules applied.</summary>
    Critical,
}

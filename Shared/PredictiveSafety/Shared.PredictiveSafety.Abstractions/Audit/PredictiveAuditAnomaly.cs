// ============================================================================
//  Project:        Shared.PredictiveSafety.Abstractions
//  Module:         Audit
//  File:           PredictiveAuditAnomaly.cs
//  Created:        2026-08-10
//  Description:    Phase 15 deliverable. The deterministic output of a
//                  predictive audit evaluation -- named "Anomaly" per this
//                  phase's own required output structure, but AnomalyDetected
//                  is a fixed-threshold count comparison (never statistical
//                  variance/mean/stddev, never ML), consistent with this
//                  phase's explicit prohibition on statistical anomaly
//                  detection.
// ============================================================================
using Shared.PredictiveSafety.Abstractions.Common;

namespace Shared.PredictiveSafety.Abstractions.Audit;

/// <summary>The deterministic output of a predictive audit evaluation.</summary>
public sealed record PredictiveAuditAnomaly(
    bool AnomalyDetected,
    PredictivePriority Priority,
    IReadOnlyList<string> AppliedRuleCodes);

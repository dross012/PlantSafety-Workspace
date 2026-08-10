// ============================================================================
//  Project:        Shared.PredictiveSafety.Infrastructure
//  Module:         Telemetry
//  File:           PredictiveMetrics.cs
//  Created:        2026-08-10
//  Description:    Phase 15 deliverable. A thin, Predictive-Safety-facing
//                  facade over the Phase 15 counters added to
//                  PlantSafetyMetrics (Shared.Telemetry.Abstractions) --
//                  deliberately does not create a second Meter instance,
//                  mirroring Shared.Scheduling.Infrastructure.Telemetry.
//                  SchedulingMetrics (Phase 14) exactly.
// ============================================================================
using System.Diagnostics.Metrics;

using Shared.Telemetry.Abstractions.Metrics;

namespace Shared.PredictiveSafety.Infrastructure.Telemetry;

/// <summary>Predictive-Safety-facing facade over the Phase 15 PlantSafetyMetrics counters. No new Meter instance.</summary>
public static class PredictiveMetrics
{
    /// <summary>Total predictive evaluations computed, tagged by domain.</summary>
    public static Counter<long> EvaluationsComputed => PlantSafetyMetrics.PredictiveEvaluationsComputed;

    /// <summary>Total predictive evaluations that required immediate escalation, tagged by domain.</summary>
    public static Counter<long> EscalationsRequired => PlantSafetyMetrics.PredictiveEscalationsRequired;

    /// <summary>Total Predictive Safety governance violations detected, tagged by rule name.</summary>
    public static Counter<long> GovernanceViolationsDetected => PlantSafetyMetrics.PredictiveGovernanceViolationsDetected;
}

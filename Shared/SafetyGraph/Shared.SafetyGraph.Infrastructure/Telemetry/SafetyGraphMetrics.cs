// ============================================================================
//  Project:        Shared.SafetyGraph.Infrastructure
//  Module:         Telemetry
//  File:           SafetyGraphMetrics.cs
//  Created:        2026-08-10
//  Description:    Phase 16 deliverable. A thin, Safety-Graph-facing facade
//                  over the Phase 16 counters added to PlantSafetyMetrics
//                  (Shared.Telemetry.Abstractions) -- deliberately does not
//                  create a second Meter instance, mirroring
//                  SchedulingMetrics/PredictiveMetrics (Phases 14/15)
//                  exactly.
// ============================================================================
using System.Diagnostics.Metrics;

using Shared.Telemetry.Abstractions.Metrics;

namespace Shared.SafetyGraph.Infrastructure.Telemetry;

/// <summary>Safety-Graph-facing facade over the Phase 16 PlantSafetyMetrics counters. No new Meter instance.</summary>
public static class SafetyGraphMetrics
{
    /// <summary>Total SafetyGraph construction operations, tagged by outcome.</summary>
    public static Counter<long> GraphsConstructed => PlantSafetyMetrics.SafetyGraphsConstructed;

    /// <summary>Total SafetyGraph traversal operations performed, tagged by traversal kind.</summary>
    public static Counter<long> TraversalsPerformed => PlantSafetyMetrics.SafetyGraphTraversalsPerformed;

    /// <summary>Total SafetyGraph inference evaluations computed, tagged by escalation outcome.</summary>
    public static Counter<long> InferencesComputed => PlantSafetyMetrics.SafetyGraphInferencesComputed;

    /// <summary>Total Safety Graph governance violations detected, tagged by rule name.</summary>
    public static Counter<long> GovernanceViolationsDetected => PlantSafetyMetrics.SafetyGraphGovernanceViolationsDetected;
}

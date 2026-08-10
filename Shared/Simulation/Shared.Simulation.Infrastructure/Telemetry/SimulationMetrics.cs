// ============================================================================
//  Project:        Shared.Simulation.Infrastructure
//  Module:         Telemetry
//  File:           SimulationMetrics.cs
//  Created:        2026-08-10
//  Description:    Phase 17 deliverable. A thin, Simulation-facing facade
//                  over the Phase 17 counters added to PlantSafetyMetrics
//                  (Shared.Telemetry.Abstractions) -- deliberately does not
//                  create a second Meter instance, mirroring
//                  SafetyGraphMetrics (Phase 16) exactly.
// ============================================================================
using System.Diagnostics.Metrics;

using Shared.Telemetry.Abstractions.Metrics;

namespace Shared.Simulation.Infrastructure.Telemetry;

/// <summary>Simulation-facing facade over the Phase 17 PlantSafetyMetrics counters. No new Meter instance.</summary>
public static class SimulationMetrics
{
    /// <summary>Total scenario replays performed, tagged by validity outcome.</summary>
    public static Counter<long> ReplaysPerformed => PlantSafetyMetrics.SimulationReplaysPerformed;

    /// <summary>Total scenario evaluations computed, tagged by escalation outcome.</summary>
    public static Counter<long> ScenariosEvaluated => PlantSafetyMetrics.SimulationScenariosEvaluated;

    /// <summary>Total Simulation governance violations detected, tagged by rule name.</summary>
    public static Counter<long> GovernanceViolationsDetected => PlantSafetyMetrics.SimulationGovernanceViolationsDetected;
}

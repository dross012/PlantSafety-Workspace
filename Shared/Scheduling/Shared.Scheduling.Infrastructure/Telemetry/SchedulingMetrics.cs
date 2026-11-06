// ============================================================================
//  Project:        Shared.Scheduling.Infrastructure
//  Module:         Telemetry
//  File:           SchedulingMetrics.cs
//  Created:        2026-08-10
//  Description:    Phase 14 (Scheduling Modernization) deliverable. A thin,
//                  Scheduling-facing facade over the Phase 14 counters added
//                  to PlantSafetyMetrics (Shared.Telemetry.Abstractions) --
//                  deliberately does not create a second Meter instance;
//                  every counter below is the same instrument every other
//                  module's telemetry wrapper publishes through, exactly
//                  the "one Meter" convention PlantSafetyMetrics' own header
//                  comment documents.
// ============================================================================
using System.Diagnostics.Metrics;

using Shared.Telemetry.Abstractions.Metrics;

namespace Shared.Scheduling.Infrastructure.Telemetry;

/// <summary>Scheduling-facing facade over the Phase 14 PlantSafetyMetrics counters. No new Meter instance.</summary>
public static class SchedulingMetrics
{
    /// <summary>Total SchedulingScheduleCalculator.Calculate invocations, tagged by whether immediate scheduling was required.</summary>
    public static Counter<long> SchedulesComputed => PlantSafetyMetrics.SchedulingSchedulesComputed;

    /// <summary>Total due-time status evaluations performed, tagged by resulting status.</summary>
    public static Counter<long> DueTimeEvaluationsPerformed => PlantSafetyMetrics.SchedulingDueTimeEvaluationsPerformed;

    /// <summary>Total scheduling evaluations that required immediate scheduling.</summary>
    public static Counter<long> ImmediateSchedulingRequiredCount => PlantSafetyMetrics.SchedulingImmediateRequiredCount;

    /// <summary>Total Scheduling-specific governance violations detected, tagged by rule name.</summary>
    public static Counter<long> GovernanceViolationsDetected => PlantSafetyMetrics.SchedulingGovernanceViolationsDetected;
}

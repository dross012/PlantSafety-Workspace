// ============================================================================
//  Project:        Deployment
//  Module:         Observability
//  File:           DeploymentMetrics.cs
//  Created:        2026-08-10
//  Description:    Phase 18 deliverable. A thin, Deployment-facing facade
//                  over the Phase 18 counters added to PlantSafetyMetrics
//                  (Shared.Telemetry.Abstractions) -- deliberately does not
//                  create a second Meter instance, mirroring every prior
//                  phase's *Metrics facade exactly.
// ============================================================================
using System.Diagnostics.Metrics;

using Shared.Telemetry.Abstractions.Metrics;

namespace Deployment.Observability;

/// <summary>Deployment-facing facade over the Phase 18 PlantSafetyMetrics counters. No new Meter instance.</summary>
public static class DeploymentMetrics
{
    /// <summary>Total deployment steps executed, tagged by outcome.</summary>
    public static Counter<long> StepsExecuted => PlantSafetyMetrics.DeploymentStepsExecuted;

    /// <summary>Total config registry snapshots published, tagged by tenant.</summary>
    public static Counter<long> SnapshotsPublished => PlantSafetyMetrics.ConfigSnapshotsPublished;

    /// <summary>Total drift reconciliation passes performed, tagged by whether drift was found.</summary>
    public static Counter<long> DriftReconciliationsPerformed => PlantSafetyMetrics.DriftReconciliationsPerformed;

    /// <summary>Total Deployment governance violations detected, tagged by rule name.</summary>
    public static Counter<long> GovernanceViolationsDetected => PlantSafetyMetrics.DeploymentGovernanceViolationsDetected;
}

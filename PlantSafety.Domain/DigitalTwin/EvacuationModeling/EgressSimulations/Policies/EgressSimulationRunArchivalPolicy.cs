// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EgressSimulations
//  Layer:          Domain
//  File:           EgressSimulationRunArchivalPolicy.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for EgressSimulationRun
//                  aggregates. Data-governance hook: simulation data must be retained 7 years, so
//                  this policy governs eligibility for cold-storage archival, never deletion.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.Policies;

/// <summary>Determines whether an <see cref="Aggregates.EgressSimulationRun"/> aggregate is eligible for archival.</summary>
public static class EgressSimulationRunArchivalPolicy
{
    /// <summary>Minimum retention period before an EgressSimulationRun may be archived to cold storage (data-governance: 7-year retention).</summary>
    public static readonly TimeSpan MinimumRetentionPeriod = TimeSpan.FromDays(365 * 7);

    /// <summary>Returns <see langword="true"/> when the run record is terminal (completed or failed) and may be archived.</summary>
    public static bool IsEligibleForArchival(Aggregates.EgressSimulationRun aggregate)
        => aggregate.Status is EgressSimulationStatus.Completed or EgressSimulationStatus.Failed;
}

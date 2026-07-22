// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.MachineProcessMonitoring
//  Layer:          Domain
//  File:           MachineProcessMonitoringArchivalPolicy.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for MachineProcessMonitoring aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.MachineProcessMonitoring.Policies;

/// <summary>Determines whether a <see cref="MachineProcessMonitoring"/> aggregate is eligible for archival.</summary>
public static class MachineProcessMonitoringArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the MachineProcessMonitoring may be archived.</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.Observability.MachineProcessMonitoring.Aggregates.MachineProcessMonitoring aggregate)
        => aggregate.Status == MachineProcessMonitoringStatus.Closed;
}
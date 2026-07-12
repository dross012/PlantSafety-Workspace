// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Schedules
//  Layer:          Domain
//  File:           SchedulesArchivalPolicy.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for Schedules aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.Schedules.Policies;

/// <summary>Determines whether a <see cref="Schedules"/> aggregate is eligible for archival.</summary>
public static class SchedulesArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the Schedules may be archived.</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.Operational.Schedules.Aggregates.Schedules aggregate)
        => aggregate.Status == SchedulesStatus.Closed;
}

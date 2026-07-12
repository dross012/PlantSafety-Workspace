// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.Observation
//  Layer:          Domain
//  File:           ObservationArchivalPolicy.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for Observation aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.Observation.Policies;

/// <summary>Determines whether a <see cref="Observation"/> aggregate is eligible for archival.</summary>
public static class ObservationArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the Observation may be archived.</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.SafetyEvents.Observation.Aggregates.Observation aggregate)
        => aggregate.Status == ObservationStatus.Closed;
}

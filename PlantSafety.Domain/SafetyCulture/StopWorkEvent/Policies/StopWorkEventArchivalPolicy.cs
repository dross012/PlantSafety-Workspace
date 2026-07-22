// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyCulture.StopWorkEvent
//  Layer:          Domain
//  File:           StopWorkEventArchivalPolicy.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for StopWorkEvent aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyCulture.StopWorkEvent.Policies;

/// <summary>Determines whether a <see cref="StopWorkEvent"/> aggregate is eligible for archival.</summary>
public static class StopWorkEventArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the StopWorkEvent may be archived.</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.SafetyCulture.StopWorkEvent.Aggregates.StopWorkEvent aggregate)
        => aggregate.Status == StopWorkEventStatus.Closed;
}
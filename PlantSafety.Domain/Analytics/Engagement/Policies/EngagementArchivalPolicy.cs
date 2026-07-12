// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Engagement
//  Layer:          Domain
//  File:           EngagementArchivalPolicy.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for Engagement aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.Engagement.Policies;

/// <summary>Determines whether a <see cref="Engagement"/> aggregate is eligible for archival.</summary>
public static class EngagementArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the Engagement may be archived.</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.Analytics.Engagement.Aggregates.Engagement aggregate)
        => aggregate.Status == EngagementStatus.Closed;
}

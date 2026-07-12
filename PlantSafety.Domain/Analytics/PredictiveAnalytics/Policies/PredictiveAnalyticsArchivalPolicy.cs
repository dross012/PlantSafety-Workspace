// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.PredictiveAnalytics
//  Layer:          Domain
//  File:           PredictiveAnalyticsArchivalPolicy.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for PredictiveAnalytics aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.PredictiveAnalytics.Policies;

/// <summary>Determines whether a <see cref="PredictiveAnalytics"/> aggregate is eligible for archival.</summary>
public static class PredictiveAnalyticsArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the PredictiveAnalytics may be archived.</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.Analytics.PredictiveAnalytics.Aggregates.PredictiveAnalytics aggregate)
        => aggregate.Status == PredictiveAnalyticsStatus.Closed;
}

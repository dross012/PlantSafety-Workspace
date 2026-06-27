// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Culture
//  Layer:          Domain
//  File:           CultureArchivalPolicy.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for Culture aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.Culture.Policies;

/// <summary>Determines whether a <see cref="Culture"/> aggregate is eligible for archival.</summary>
public static class CultureArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the Culture may be archived.</summary>
    public static bool IsEligibleForArchival(Culture aggregate)
        => aggregate.Status == CultureStatus.Closed;
}

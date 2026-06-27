// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.ScoreCards
//  Layer:          Domain
//  File:           ScoreCardsArchivalPolicy.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for ScoreCards aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.ScoreCards.Policies;

/// <summary>Determines whether a <see cref="ScoreCards"/> aggregate is eligible for archival.</summary>
public static class ScoreCardsArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the ScoreCards may be archived.</summary>
    public static bool IsEligibleForArchival(ScoreCards aggregate)
        => aggregate.Status == ScoreCardsStatus.Closed;
}

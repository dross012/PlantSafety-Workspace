// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scorecards
//  Layer:          Domain
//  File:           ScorecardArchivalPolicy.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for Scorecard aggregates.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Scorecards.Aggregates;
using PlantSafety.Domain.Scorecards.Enums;

namespace PlantSafety.Domain.Scorecards.Policies;

/// <summary>Determines whether a Scorecard aggregate is eligible for archival.</summary>
public static class ScorecardArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the Scorecard may be archived.</summary>
    public static bool IsEligibleForArchival(Scorecard aggregate)
        => aggregate.Status == ScorecardStatus.Closed;
}
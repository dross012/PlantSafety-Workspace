// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityConcern
//  Layer:          Domain
//  File:           CommunityConcernArchivalPolicy.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for CommunityConcern aggregates.
//                  Implements the module's data-governance retention rule: resolved concern
//                  records are retained per the public-records/regulatory retention requirement
//                  before they become eligible for archival (retention window supplied by the
//                  caller; this policy makes no infrastructure/clock calls itself). Additive-only
//                  evolution.
// ============================================================================
using PlantSafety.Domain.CommunitySafety.CommunityConcern.Enums;
using Agg = PlantSafety.Domain.CommunitySafety.CommunityConcern.Aggregates.CommunityConcern;

namespace PlantSafety.Domain.CommunitySafety.CommunityConcern.Policies;

/// <summary>Determines whether a <see cref="Agg"/> aggregate is eligible for archival.</summary>
public static class CommunityConcernArchivalPolicy
{
    /// <summary>Minimum retention period for a resolved concern before archival is permitted (public-records/regulatory retention requirement).</summary>
    public static readonly TimeSpan MinimumRetentionAfterResolution = TimeSpan.FromDays(365 * 3);

    /// <summary>Returns <see langword="true"/> when the CommunityConcern may be archived as of <paramref name="nowUtc"/>.</summary>
    public static bool IsEligibleForArchival(Agg aggregate, UtcTimestamp nowUtc)
        => aggregate.Status == CommunityConcernStatus.Resolved
           && aggregate.ResolvedAtUtc is not null
           && nowUtc.Value - aggregate.ResolvedAtUtc.Value.Value >= MinimumRetentionAfterResolution;
}

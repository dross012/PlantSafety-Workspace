// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityConcern
//  Layer:          Domain
//  File:           CommunityConcernLifecycleRules.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Pure, stateless lifecycle transition rules for the CommunityConcern aggregate.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CommunitySafety.CommunityConcern.Enums;

namespace PlantSafety.Domain.CommunitySafety.CommunityConcern.Rules;

/// <summary>Pure lifecycle transition rules for the <see cref="Aggregates.CommunityConcern"/> aggregate.</summary>
public static class CommunityConcernLifecycleRules
{
    /// <summary>Returns whether a transition from <paramref name="from"/> to <paramref name="to"/> is permitted.</summary>
    public static bool CanTransition(CommunityConcernStatus from, CommunityConcernStatus to) => (from, to) switch
    {
        (CommunityConcernStatus.Open, CommunityConcernStatus.UnderReview) => true,
        (CommunityConcernStatus.Open, CommunityConcernStatus.Escalated) => true,
        (CommunityConcernStatus.Open, CommunityConcernStatus.Resolved) => true,
        (CommunityConcernStatus.UnderReview, CommunityConcernStatus.Escalated) => true,
        (CommunityConcernStatus.UnderReview, CommunityConcernStatus.Resolved) => true,
        (CommunityConcernStatus.Escalated, CommunityConcernStatus.UnderReview) => true,
        (CommunityConcernStatus.Escalated, CommunityConcernStatus.Resolved) => true,
        (CommunityConcernStatus.Resolved, CommunityConcernStatus.Archived) => true,
        _ => false,
    };

    /// <summary>Bumps a free-text severity string one tier: Low -&gt; Medium -&gt; High -&gt; Critical (saturating).</summary>
    public static string BumpSeverity(string currentSeverity) => (currentSeverity ?? string.Empty).Trim().ToLowerInvariant() switch
    {
        "low" => "Medium",
        "medium" => "High",
        "high" => "Critical",
        _ => "Critical",
    };
}

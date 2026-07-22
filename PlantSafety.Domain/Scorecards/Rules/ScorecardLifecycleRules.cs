// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scorecards
//  Layer:          Domain
//  File:           ScorecardLifecycleRules.cs
//  Created:        2026-07-20
//  Updated:        2026-07-21 -- realigned with the real compilation/distribution lifecycle
//                  (Draft -> Compiled -> Published -> Closed -> Archived).
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the Scorecard aggregate.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Scorecards.Enums;

namespace PlantSafety.Domain.Scorecards.Rules;

/// <summary>Defines permitted lifecycle transitions for the Scorecard aggregate.</summary>
public static class ScorecardLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when a KPI measurement may be recorded from <paramref name="current"/>.</summary>
    public static bool CanRecordMetric(ScorecardStatus current) => current is ScorecardStatus.Draft or ScorecardStatus.Compiled;

    /// <summary>Returns <see langword="true"/> when the scorecard may be published from <paramref name="current"/>.</summary>
    public static bool CanPublish(ScorecardStatus current) => current == ScorecardStatus.Compiled;

    /// <summary>Returns <see langword="true"/> when the scorecard may be closed from <paramref name="current"/>.</summary>
    public static bool CanClose(ScorecardStatus current) => current == ScorecardStatus.Published;

    /// <summary>Returns <see langword="true"/> when the aggregate may be archived from <paramref name="current"/>.</summary>
    public static bool CanArchive(ScorecardStatus current) => current == ScorecardStatus.Closed;
}

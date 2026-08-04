// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityConcern
//  Layer:          Domain
//  File:           CommunityConcernService.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain service for CommunityConcern. Contains pure, stateless logic for
//                  detecting a recurring-concern pattern within a facility area over a rolling
//                  window (feeds the repeat-concern-rate KPI and the RiskManagement escalation
//                  risk hook). Accepts only Value Objects/primitives already loaded by the
//                  caller; makes no repository, HTTP, or SQL calls itself, per the Canonical
//                  Domain Service Rule. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CommunitySafety.CommunityConcern.Enums;

namespace PlantSafety.Domain.CommunitySafety.CommunityConcern.Services;

/// <summary>Domain service for CommunityConcern recurring-pattern detection.</summary>
public sealed class CommunityConcernService : IDomainService
{
    /// <summary>
    /// Default rolling window used to evaluate whether a facility area has a recurring-concern
    /// pattern warranting escalation and elevated RiskManagement visibility.
    /// </summary>
    public static readonly TimeSpan DefaultRecurrenceWindow = TimeSpan.FromDays(90);

    /// <summary>
    /// Minimum number of concerns for the same facility area within
    /// <see cref="DefaultRecurrenceWindow"/> that constitutes a recurring pattern.
    /// </summary>
    public const int RecurrenceThreshold = 3;

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="recentConcernTimestampsForArea"/> (already
    /// filtered by the caller to a single facility area) contains at least
    /// <see cref="RecurrenceThreshold"/> entries within <see cref="DefaultRecurrenceWindow"/> of
    /// <paramref name="nowUtc"/>, indicating a recurring-concern pattern.
    /// </summary>
    public static bool IsRecurringPattern(IReadOnlyCollection<UtcTimestamp> recentConcernTimestampsForArea, UtcTimestamp nowUtc)
    {
        var windowStart = nowUtc.Value - DefaultRecurrenceWindow;
        var countInWindow = recentConcernTimestampsForArea.Count(t => t.Value >= windowStart && t.Value <= nowUtc.Value);
        return countInWindow >= RecurrenceThreshold;
    }

    /// <summary>Computes the repeat-concern rate (fraction of concerns that are part of a recurring pattern) for a facility area.</summary>
    public static decimal ComputeRepeatConcernRate(int totalConcernsForArea, int recurringConcernsForArea)
        => totalConcernsForArea <= 0 ? 0m : Math.Round((decimal)recurringConcernsForArea / totalConcernsForArea, 4);
}

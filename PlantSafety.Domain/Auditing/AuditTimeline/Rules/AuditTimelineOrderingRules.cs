// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditTimeline
//  Layer:          Domain
//  File:           AuditTimelineOrderingRules.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Defines the deterministic ordering rules used to merge heterogeneous
//                  AuditTimelineEntry sources into a single chronological view. Additive-only
//                  evolution.
// ============================================================================
using PlantSafety.Domain.Auditing.AuditTimeline.ValueObjects;

namespace PlantSafety.Domain.Auditing.AuditTimeline.Rules;

/// <summary>Defines the deterministic ordering rules for merging AuditTimeline entries.</summary>
public static class AuditTimelineOrderingRules
{
    /// <summary>
    /// Orders entries strictly ascending by <see cref="AuditTimelineEntry.UtcTimestamp"/>. Ties are
    /// broken deterministically by <see cref="AuditTimelineEntry.AuditTimelineEntryId"/> so
    /// repeated reconstructions of the same underlying data always produce the same sequence.
    /// </summary>
    public static IReadOnlyList<AuditTimelineEntry> OrderChronologically(IEnumerable<AuditTimelineEntry> entries)
        => entries
            .OrderBy(e => e.UtcTimestamp.Value)
            .ThenBy(e => e.AuditTimelineEntryId.Value)
            .ToList();
}

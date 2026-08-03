// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditTimeline
//  Layer:          Domain
//  File:           AuditTimelineCompositionService.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Pure domain service that reconstructs a single coherent, strictly
//                  UtcTimestamp-ordered AuditTimeline from heterogeneous entry sources (domain
//                  events, state-change snapshots, concurrency conflicts, and scheduling
//                  decisions). Contains no infrastructure access -- Application-layer query
//                  handlers gather the raw entries from their respective repositories and hand
//                  them to this service for deterministic composition (Handbook Phase 5 §6.1).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Auditing.AuditTimeline.Rules;
using PlantSafety.Domain.Auditing.AuditTimeline.ValueObjects;

namespace PlantSafety.Domain.Auditing.AuditTimeline.Services;

/// <summary>Pure domain service that composes a single chronological AuditTimeline from heterogeneous sources.</summary>
public sealed class AuditTimelineCompositionService
{
    /// <summary>
    /// Merges every supplied entry source into one strictly chronological (UtcTimestamp,
    /// then EntryId) sequence via <see cref="AuditTimelineOrderingRules.OrderChronologically"/>.
    /// </summary>
    public IReadOnlyList<AuditTimelineEntry> Compose(params IEnumerable<AuditTimelineEntry>[] sources)
        => AuditTimelineOrderingRules.OrderChronologically(sources.SelectMany(s => s));
}

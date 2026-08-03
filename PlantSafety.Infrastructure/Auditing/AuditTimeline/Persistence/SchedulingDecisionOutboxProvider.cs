// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditTimeline
//  Layer:          Infrastructure
//  File:           SchedulingDecisionOutboxProvider.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Implements ISchedulingDecisionHistoryProvider over the shared
//                  OutboxTimelineEntryReader, satisfying the requirement to link Phase 5 §1's
//                  scheduling orchestration decisions into AuditTimeline "via SharedKernel
//                  contracts/events" WITHOUT a hard cross-module Domain/Application dependency on
//                  PlantSafety.Domain.Scheduling or PlantSafety.Application.Scheduling (Handbook
//                  Part IX Canonical Dependency Rules / Part IV §4.2.2 Zero Cross-Module
//                  Leakage). SchedulingDecisionEventTypePrefixes is a small, self-contained
//                  Application-level "integration contract" naming convention (matching the
//                  Scheduling module's own event-naming pattern of
//                  "&lt;AggregateName&gt;&lt;Verb&gt;", e.g. "PMSchedulingEscalated") rather than a
//                  compiled reference to any Scheduling type -- if the Scheduling module renames
//                  its aggregates this list needs a manual update, but no assembly reference ever
//                  exists between the two modules. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Auditing.AuditTimeline.ValueObjects;
using PlantSafety.Application.Auditing.AuditTimeline.Repositories;

namespace PlantSafety.Infrastructure.Auditing.AuditTimeline.Persistence;

/// <summary>Outbox-backed implementation of <see cref="ISchedulingDecisionHistoryProvider"/>.</summary>
public sealed class SchedulingDecisionOutboxProvider(OutboxTimelineEntryReader reader) : ISchedulingDecisionHistoryProvider
{
    /// <summary>
    /// Event-type name prefixes recognized as Scheduling-module orchestration decisions, per the
    /// module's own <c>&lt;AggregateName&gt;&lt;Verb&gt;</c> event-naming convention (see
    /// PlantSafety.Domain.Scheduling.*.Events).
    /// </summary>
    private static readonly string[] SchedulingDecisionEventTypePrefixes =
    [
        "PMScheduling",
        "WorkOrderPriority",
        "EmergencyDrillPlanning",
        "HazardIsolationSequencing",
        "ShiftFatigue",
    ];

    /// <inheritdoc/>
    public async Task<IReadOnlyList<AuditTimelineEntry>> GetSchedulingDecisionEntriesAsync(Guid tenantId, string entityType, Guid entityId, CancellationToken cancellationToken)
    {
        var rows = await reader.GetRowsForEntityAsync(tenantId, entityId, cancellationToken);

        return rows
            .Where(r => SchedulingDecisionEventTypePrefixes.Any(prefix => r.EventType.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
            .Select(r => OutboxTimelineEntryReader.ToSchedulingDecisionEntry(r, entityType, entityId))
            .ToList();
    }
}

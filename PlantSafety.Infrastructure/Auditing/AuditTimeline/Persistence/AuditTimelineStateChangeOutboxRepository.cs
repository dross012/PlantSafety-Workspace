// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditTimeline
//  Layer:          Infrastructure
//  File:           AuditTimelineStateChangeOutboxRepository.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Implements IAuditTimelineStateChangeRepository over the shared
//                  OutboxTimelineEntryReader. This platform does not maintain a separate
//                  persisted "snapshot per mutation" table for every aggregate -- each module's
//                  Snapshots/ convention (e.g. RiskSnapshot) is an in-memory rehydration DTO
//                  shape, not a historized table -- so state-change history is derived from the
//                  subset of an entity's own domain events whose payload demonstrably carries
//                  before/after state (a "PreviousStatus"/"NewStatus"/"Status" JSON property),
//                  which is the closest real, queryable signal of a recorded state transition
//                  available anywhere in the platform today. This is flagged as an assumption in
//                  the Phase 5 §6.1 delivery summary. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Auditing.AuditTimeline.ValueObjects;
using PlantSafety.Application.Auditing.AuditTimeline.Repositories;

namespace PlantSafety.Infrastructure.Auditing.AuditTimeline.Persistence;

/// <summary>Outbox-backed implementation of <see cref="IAuditTimelineStateChangeRepository"/>.</summary>
public sealed class AuditTimelineStateChangeOutboxRepository(OutboxTimelineEntryReader reader) : IAuditTimelineStateChangeRepository
{
    private static readonly string[] StateChangeSignalProperties = ["PreviousStatus", "NewStatus", "\"Status\":", "PreviousStatus", "Status\":"];

    /// <inheritdoc/>
    public async Task<IReadOnlyList<AuditTimelineEntry>> GetStateChangeEntriesAsync(Guid tenantId, string entityType, Guid entityId, CancellationToken cancellationToken)
    {
        var rows = await reader.GetRowsForEntityAsync(tenantId, entityId, cancellationToken);

        return rows
            .Where(r => StateChangeSignalProperties.Any(signal => r.Payload.Contains(signal, StringComparison.OrdinalIgnoreCase)))
            .Select(r => OutboxTimelineEntryReader.ToStateChangeEntry(r, entityType, entityId))
            .ToList();
    }
}

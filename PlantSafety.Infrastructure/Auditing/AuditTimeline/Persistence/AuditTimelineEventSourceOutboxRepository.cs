// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditTimeline
//  Layer:          Infrastructure
//  File:           AuditTimelineEventSourceOutboxRepository.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Implements IAuditTimelineEventSourceRepository over the shared
//                  OutboxTimelineEntryReader: every domain event an entity raised, as recorded in
//                  the durable Outbox. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Auditing.AuditTimeline.ValueObjects;
using PlantSafety.Application.Auditing.AuditTimeline.Repositories;

namespace PlantSafety.Infrastructure.Auditing.AuditTimeline.Persistence;

/// <summary>Outbox-backed implementation of <see cref="IAuditTimelineEventSourceRepository"/>.</summary>
public sealed class AuditTimelineEventSourceOutboxRepository(OutboxTimelineEntryReader reader) : IAuditTimelineEventSourceRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<AuditTimelineEntry>> GetEventEntriesAsync(Guid tenantId, string entityType, Guid entityId, CancellationToken cancellationToken)
    {
        var rows = await reader.GetRowsForEntityAsync(tenantId, entityId, cancellationToken);
        return rows.Select(r => OutboxTimelineEntryReader.ToEventEntry(r, entityType, entityId)).ToList();
    }
}

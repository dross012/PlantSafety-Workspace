// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: Read-only Application-layer abstraction returning the "event timeline"
//   source for AuditTimeline reconstruction (Handbook Phase 5 §6.1): every domain event a given
//   entity/aggregate raised, sourced from the durable Outbox event log. Implemented in
//   Infrastructure against [System].[OutboxMessages] so it works uniformly across every module
//   without a hard per-module dependency.
// SchemaVersion: 1

using PlantSafety.Domain.Auditing.AuditTimeline.ValueObjects;

namespace PlantSafety.Application.Auditing.AuditTimeline.Repositories;

/// <summary>Read-only repository returning the event-raised entries for AuditTimeline reconstruction.</summary>
public interface IAuditTimelineEventSourceRepository
{
    /// <summary>Returns every domain event raised by the given entity, as normalized AuditTimelineEntry values.</summary>
    Task<IReadOnlyList<AuditTimelineEntry>> GetEventEntriesAsync(
        Guid tenantId, string entityType, Guid entityId, CancellationToken cancellationToken);
}

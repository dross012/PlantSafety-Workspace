// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: Read-only Application-layer abstraction returning the "state change
//   history" source for AuditTimeline reconstruction (Handbook Phase 5 §6.1): before/after
//   snapshot pairs recorded for a given entity/aggregate, per the module's own Snapshots
//   convention.
// SchemaVersion: 1

using PlantSafety.Domain.Auditing.AuditTimeline.ValueObjects;

namespace PlantSafety.Application.Auditing.AuditTimeline.Repositories;

/// <summary>Read-only repository returning the state-change entries for AuditTimeline reconstruction.</summary>
public interface IAuditTimelineStateChangeRepository
{
    /// <summary>Returns every recorded before/after state-change entry for the given entity.</summary>
    Task<IReadOnlyList<AuditTimelineEntry>> GetStateChangeEntriesAsync(
        Guid tenantId, string entityType, Guid entityId, CancellationToken cancellationToken);
}

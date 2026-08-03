// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: Read-only Application-layer abstraction returning the "scheduling decision
//   history" source for AuditTimeline reconstruction (Handbook Phase 5 §6.1). Deliberately
//   defined here (Auditing module) rather than referencing PlantSafety.Domain.Scheduling or
//   PlantSafety.Application.Scheduling directly, per Handbook Part IX Canonical Dependency Rules
//   and Part IV §4.2.2 Zero Cross-Module Leakage: AuditTimeline must not take a hard
//   Domain-to-Domain dependency on the Scheduling module's Predictive Scheduling rule engine
//   (Phase 5 §1, built independently by another agent). Instead this abstraction is satisfied by
//   an Infrastructure implementation that reads scheduling-orchestration decisions exclusively
//   through the shared, cross-module SharedKernel event contracts (IIntegrationEvent /
//   IDomainEvent) durably persisted in the Outbox -- i.e. the same integration-event boundary
//   every other cross-module consumer in this platform uses.
// SchemaVersion: 1

using PlantSafety.Domain.Auditing.AuditTimeline.ValueObjects;

namespace PlantSafety.Application.Auditing.AuditTimeline.Repositories;

/// <summary>Read-only provider returning scheduling-decision entries for AuditTimeline reconstruction.</summary>
public interface ISchedulingDecisionHistoryProvider
{
    /// <summary>
    /// Returns every scheduling orchestration decision (Phase 5 §1) linked to the given entity,
    /// sourced from cross-module integration/domain events rather than a direct Scheduling
    /// module dependency. Returns an empty list when no scheduling decisions are linked or the
    /// Scheduling module has not yet published any for this entity.
    /// </summary>
    Task<IReadOnlyList<AuditTimelineEntry>> GetSchedulingDecisionEntriesAsync(
        Guid tenantId, string entityType, Guid entityId, CancellationToken cancellationToken);
}

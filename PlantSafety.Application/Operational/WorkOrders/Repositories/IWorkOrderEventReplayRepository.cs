// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.WorkOrders
//  Layer:          Application
//  File:           IWorkOrderEventReplayRepository.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 6 §3.4 Replay/Rebuild Across Systems, Eventing & Messaging Agent)
//  SchemaVersion:  2026.1
//  Description:    Read-only repository abstraction that returns the raw, chronologically ordered
//                  WorkOrder domain-event history directly from the platform's real
//                  <c>[System].[OutboxMessages]</c> table (the same table
//                  <see cref="PlantSafety.Infrastructure.BackgroundJobs.OutboxProcessor"/> already
//                  publishes from) -- extending, not duplicating, the existing Outbox-based history
//                  mechanism to support point-in-time state reconstruction (see
//                  <c>RebuildWorkOrderTimelineQuery</c>). Distinct from
//                  <see cref="IWorkOrderReadRepository.GetHistoryAsync"/>, which returns a
//                  human-readable, already-summarised projection for the Maintenance UI; this
//                  repository returns the raw EventType/Payload pairs a replay/rebuild handler needs
//                  to reconstruct actual field-level state. Implemented in the Infrastructure layer.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Operational.WorkOrders.Repositories;

/// <summary>One raw, chronologically-ordered WorkOrder domain-event record read directly from <c>[System].[OutboxMessages]</c>.</summary>
/// <param name="EventType">The Outbox <c>EventType</c> discriminator (e.g. <c>"WorkOrderCreated"</c>).</param>
/// <param name="OccurredAtUtc">UTC timestamp the event occurred.</param>
/// <param name="PayloadJson">The raw JSON payload as stored in the Outbox.</param>
public sealed record WorkOrderReplayEventRecord(string EventType, DateTime OccurredAtUtc, string PayloadJson);

/// <summary>
/// Read-only repository returning a WorkOrder's raw domain-event history straight from
/// <c>[System].[OutboxMessages]</c>, for replay/rebuild purposes.
/// </summary>
public interface IWorkOrderEventReplayRepository
{
    /// <summary>
    /// Returns every WorkOrder lifecycle event recorded in the Outbox for <paramref name="workOrderId"/>
    /// within <paramref name="tenantId"/>, ordered chronologically (oldest first).
    /// </summary>
    Task<IReadOnlyList<WorkOrderReplayEventRecord>> GetRawEventsAsync(
        Guid tenantId, Guid workOrderId, CancellationToken cancellationToken);
}

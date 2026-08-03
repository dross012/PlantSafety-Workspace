// AUTHOR: PlantSafety Platform Team (Eventing & Messaging Agent, Phase 6)
// CREATED: 2026-08-02
// FILE DESCRIPTION: Repository abstraction over [System].[DeadLetterEvents] -- the MassTransit
//   fault-pipeline destination table. Read methods back the DLQ observability queries; ReplayAsync
//   is implemented in the Infrastructure layer because replay requires re-publishing onto the
//   MassTransit bus (a concrete transport concern that must not leak into the Application layer --
//   see Canonical Dependency Rules §9.1/§9.3), while remaining a single atomic repository
//   operation from the Application layer's perspective.
// SchemaVersion: 1

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using PlantSafety.Application.Messaging.Models;

namespace PlantSafety.Application.Messaging.Repositories;

/// <summary>Repository over the MassTransit dead-letter queue analytics/replay surface.</summary>
public interface IDeadLetterEventRepository
{
    /// <summary>
    /// Returns dead-lettered events, most recently faulted first, with optional tenant/event-type
    /// filters and a switch to include or exclude already-replayed rows.
    /// </summary>
    /// <param name="tenantId">Optional exact match on TenantId. Null returns all tenants.</param>
    /// <param name="eventType">Optional exact match on EventType. Null returns all event types.</param>
    /// <param name="includeReplayed">When <see langword="false"/>, excludes rows that have already been replayed.</param>
    /// <param name="top">Page size.</param>
    /// <param name="skip">Rows to skip for pagination.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task<IReadOnlyList<DeadLetterEventDto>> GetRecentAsync(
        string? tenantId, string? eventType, bool includeReplayed, int top, int skip,
        CancellationToken cancellationToken);

    /// <summary>Returns every dead-lettered event sharing <paramref name="correlationId"/>, oldest first.</summary>
    Task<IReadOnlyList<DeadLetterEventDto>> GetByCorrelationIdAsync(
        Guid correlationId, CancellationToken cancellationToken);

    /// <summary>Returns dead-letter counts/ages grouped by EventType over the trailing <paramref name="lookbackDays"/>.</summary>
    /// <param name="tenantId">Optional exact match on TenantId. Null returns all tenants.</param>
    /// <param name="lookbackDays">How many days back to include.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task<IReadOnlyList<DeadLetterEventsSummaryDto>> GetSummaryAsync(
        string? tenantId, int lookbackDays, CancellationToken cancellationToken);

    /// <summary>Returns a single dead-lettered event by id, or <see langword="null"/> if not found.</summary>
    Task<DeadLetterEventDto?> GetByIdAsync(Guid deadLetterId, CancellationToken cancellationToken);

    /// <summary>
    /// Re-publishes the dead-lettered event's original payload back onto the message bus (resolving
    /// its concrete CLR type via the same outbox event-type registry the OutboxProcessor uses), then
    /// marks the row as replayed in the same logical operation. Idempotent: replaying an
    /// already-replayed row returns <see langword="false"/> without re-publishing.
    /// </summary>
    /// <param name="deadLetterId">The dead-letter row to replay.</param>
    /// <param name="replayedByUserId">The user who triggered the replay (Domain Action Identity Rule §1.8.2).</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    /// <returns><see langword="true"/> if the event was re-published and the row marked replayed; <see langword="false"/> if the row did not exist or was already replayed.</returns>
    Task<bool> ReplayAsync(Guid deadLetterId, Guid replayedByUserId, CancellationToken cancellationToken);
}

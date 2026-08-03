// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// UPDATED: 2026-07-27 -- CreateAsync/UpdateAsync now take the Domain
//   IncidentManagement aggregate (built/rehydrated by the handler via
//   IncidentManagement.Create/Rehydrate) instead of raw primitives, so that the
//   aggregate's invariants and domain events are the single source of truth for the
//   write path. UpdateAsync additionally keeps the original null-means-unchanged
//   diff parameters (title/description/severity/location/assignedInvestigatorId) so the
//   Infrastructure implementation can still target the existing ISNULL/COALESCE partial-
//   update stored procedure rather than overwriting every column.
// FILE DESCRIPTION: Write-side repository interface for the Incident aggregate.
//   Implemented in PlantSafety.Infrastructure (stored procedures, Dapper).
//   Application layer depends only on this interface — no EF Core, no inline SQL.
// SchemaVersion: 2

using IncidentAggregate = PlantSafety.Domain.SafetyEvents.IncidentManagement.Aggregates.IncidentManagement;

namespace PlantSafety.Application.Features.SafetyEvents.Handlers.IncidentManagement;

/// <summary>
/// Write-side repository for the Incident aggregate.
/// All mutations use parameterised stored procedures via the Infrastructure implementation.
/// Methods are called only inside an open UoW transaction (enforced by TransactionBehavior).
/// </summary>
public interface IIncidentRepository
{
    /// <summary>
    /// Persists a newly <see cref="IncidentAggregate.Create"/>-d aggregate.
    /// The Infrastructure implementation enqueues the aggregate's pending domain events
    /// (e.g. <c>IncidentManagementCreatedEvent</c>) to the outbox after a successful write.
    /// </summary>
    Task CreateAsync(
        IncidentAggregate aggregate,
        Guid actorId,
        Guid correlationId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Persists the result of calling <see cref="IncidentAggregate.UpdateDetails"/> on a
    /// <see cref="IncidentAggregate.Rehydrate"/>-d aggregate. <paramref name="title"/>/
    /// <paramref name="description"/>/<paramref name="severity"/>/<paramref name="location"/>/
    /// <paramref name="assignedInvestigatorId"/> mirror the original null-means-unchanged command
    /// payload so the Infrastructure implementation can keep using the existing ISNULL/COALESCE
    /// partial-update stored procedure. Only fields with non-null values are updated.
    /// The Infrastructure implementation enqueues the aggregate's pending domain events
    /// (e.g. <c>IncidentManagementUpdatedEvent</c>) to the outbox after a successful write.
    /// Throws <see cref="Common.Errors.ConcurrencyConflictException"/> when ETag is stale.
    /// </summary>
    Task UpdateAsync(
        IncidentAggregate aggregate,
        string? title,
        string? description,
        string? severity,
        string? location,
        Guid? assignedInvestigatorId,
        string eTag,
        Guid actorId,
        Guid correlationId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Soft-deletes an Incident by transitioning its status to <c>Archived</c>.
    /// Throws <see cref="Common.Errors.ConcurrencyConflictException"/> when ETag is stale.
    /// </summary>
    Task SoftDeleteAsync(
        Guid tenantId,
        Guid incidentId,
        string deletionReason,
        string eTag,
        Guid actorId,
        Guid correlationId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Persists the result of calling <see cref="IncidentAggregate.AssignTechnician"/> on a
    /// rehydrated aggregate: updates AssignedInvestigatorId (and Status, if it advanced from
    /// Reported to Assigned), enqueues pending domain events, and persists any pending timeline
    /// entries. Added 2026-08-03 (UI Gaps Scoping, IncidentManagement Part 1).
    /// </summary>
    Task AssignAsync(
        IncidentAggregate aggregate,
        Guid actorId,
        Guid correlationId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Persists the result of calling <see cref="IncidentAggregate.Escalate"/> on a rehydrated
    /// aggregate: updates Status/Severity/EscalationReason/EscalatedAtUtc/EscalationCount,
    /// enqueues pending domain events, and persists any pending timeline entries.
    /// </summary>
    Task EscalateAsync(
        IncidentAggregate aggregate,
        Guid actorId,
        Guid correlationId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Persists the result of calling <see cref="IncidentAggregate.AddNote"/> on a rehydrated
    /// aggregate: enqueues pending domain events and persists the pending timeline entry. Does
    /// not mutate the Incident row itself (notes have no dedicated column; the timeline entry is
    /// the record of truth).
    /// </summary>
    Task AddNoteAsync(
        IncidentAggregate aggregate,
        Guid actorId,
        Guid correlationId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Persists the result of calling <see cref="IncidentAggregate.Resolve"/> on a rehydrated
    /// aggregate: updates Status/ResolutionNotes/ClosedAtUtc, enqueues pending domain events, and
    /// persists any pending timeline entries.
    /// </summary>
    Task ResolveAsync(
        IncidentAggregate aggregate,
        Guid actorId,
        Guid correlationId,
        CancellationToken cancellationToken);
}

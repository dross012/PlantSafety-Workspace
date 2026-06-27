// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Write-side repository interface for the Incident aggregate.
//   Implemented in PlantSafety.Infrastructure (stored procedures, Dapper).
//   Application layer depends only on this interface — no EF Core, no inline SQL.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.SafetyEvents.Handlers.IncidentManagement;

/// <summary>
/// Write-side repository for the Incident aggregate.
/// All mutations use parameterised stored procedures via the Infrastructure implementation.
/// Methods are called only inside an open UoW transaction (enforced by TransactionBehavior).
/// </summary>
public interface IIncidentRepository
{
    /// <summary>
    /// Creates a new Incident record.
    /// Writes an outbox envelope for <c>IncidentCreatedEvent</c> atomically within the transaction.
    /// </summary>
    Task CreateAsync(
        Guid incidentId,
        Guid tenantId,
        Guid siteId,
        string title,
        string description,
        DateTimeOffset incidentOccurredAtUtc,
        string severity,
        string location,
        Guid reportedByWorkerId,
        Guid? assignedInvestigatorId,
        Guid actorId,
        Guid correlationId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Updates mutable fields on an existing Incident.
    /// Only fields with non-null values are updated.
    /// Throws <see cref="Common.Errors.ConcurrencyConflictException"/> when ETag is stale.
    /// </summary>
    Task UpdateAsync(
        Guid tenantId,
        Guid incidentId,
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
}

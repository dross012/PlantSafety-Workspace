// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-side DTO for a Corrective and Preventive Action (CAPA) record.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.ContinuousImprovement.Models.CAPA;

/// <summary>
/// Read-side projection of a CorrectiveAction (CAPA) aggregate.
/// CAPAs are formal actions to correct a root cause or prevent recurrence of a safety event.
/// </summary>
public sealed record CorrectiveActionDto
{
    /// <summary>Unique identifier (UUIDv7).</summary>
    public required Guid CorrectiveActionId { get; init; }

    /// <summary>Tenant owning this record.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site where the corrective action is to be implemented.</summary>
    public required Guid SiteId { get; init; }

    /// <summary>Title. Max 200 characters.</summary>
    public required string Title { get; init; }

    /// <summary>Description of the corrective action. Max 4000 characters.</summary>
    public required string Description { get; init; }

    /// <summary>Type: Corrective (fixes a found problem) or Preventive (prevents recurrence).</summary>
    public required string ActionType { get; init; }

    /// <summary>Source event that triggered this CAPA (e.g., IncidentId, AuditFindingId, NearMissId).</summary>
    public required Guid SourceEventId { get; init; }

    /// <summary>Type of source event (Incident, NearMiss, AuditFinding, Observation).</summary>
    public required string SourceEventType { get; init; }

    /// <summary>Priority (Low, Medium, High, Critical).</summary>
    public required string Priority { get; init; }

    /// <summary>Assigned to this worker for implementation.</summary>
    public required Guid AssignedToWorkerId { get; init; }

    /// <summary>Workflow status (e.g., Open, InProgress, AwaitingVerification, Closed, Overdue).</summary>
    public required string Status { get; init; }

    /// <summary>UTC due date for completion.</summary>
    public required DateTimeOffset DueAtUtc { get; init; }

    /// <summary>UTC timestamp when the action was completed, if applicable.</summary>
    public DateTimeOffset? CompletedAtUtc { get; init; }

    /// <summary>UTC timestamp of last modification.</summary>
    public required DateTimeOffset LastModifiedAtUtc { get; init; }

    /// <summary>Row version for concurrency.</summary>
    public required string ETag { get; init; }
}

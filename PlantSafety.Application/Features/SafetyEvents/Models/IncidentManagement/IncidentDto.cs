// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-side data transfer object for an Incident record.
//   Used by all Get/List/Search query responses in the IncidentManagement sub-feature.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.SafetyEvents.Models.IncidentManagement;

/// <summary>
/// Read-side projection of an Incident aggregate.
/// This record is immutable; all mutations occur via Commands.
/// </summary>
public sealed record IncidentDto
{
    /// <summary>Unique identifier of the incident (UUIDv7).</summary>
    public required Guid IncidentId { get; init; }

    /// <summary>Tenant owning this incident.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site where the incident occurred.</summary>
    public required Guid SiteId { get; init; }

    /// <summary>Short descriptive title of the incident.</summary>
    public required string Title { get; init; }

    /// <summary>Detailed description of what occurred.</summary>
    public required string Description { get; init; }

    /// <summary>UTC timestamp when the incident occurred.</summary>
    public required DateTimeOffset IncidentOccurredAtUtc { get; init; }

    /// <summary>UTC timestamp when the incident was reported.</summary>
    public required DateTimeOffset ReportedAtUtc { get; init; }

    /// <summary>Severity classification (e.g., Minor, Moderate, Serious, Critical).</summary>
    public required string Severity { get; init; }

    /// <summary>Current workflow status (e.g., Reported, UnderInvestigation, Closed).</summary>
    public required string Status { get; init; }

    /// <summary>Physical or logical location where the incident occurred.</summary>
    public required string Location { get; init; }

    /// <summary>Worker who reported the incident.</summary>
    public required Guid ReportedByWorkerId { get; init; }

    /// <summary>Worker assigned to investigate, if any.</summary>
    public Guid? AssignedInvestigatorId { get; init; }

    /// <summary>UTC timestamp when investigation was closed, if applicable.</summary>
    public DateTimeOffset? ClosedAtUtc { get; init; }

    /// <summary>Actor who last modified this record.</summary>
    public required Guid LastModifiedByActorId { get; init; }

    /// <summary>UTC timestamp of the last modification.</summary>
    public required DateTimeOffset LastModifiedAtUtc { get; init; }

    /// <summary>Row version for optimistic concurrency checks.</summary>
    public required string ETag { get; init; }

    /// <summary>Reason recorded for the most recent escalation, if any. Added 2026-08-03.</summary>
    public string? EscalationReason { get; init; }

    /// <summary>Number of times this incident has been escalated. Added 2026-08-03.</summary>
    public int EscalationCount { get; init; }

    /// <summary>Resolution notes recorded when the incident was resolved, if any. Added 2026-08-03.</summary>
    public string? ResolutionNotes { get; init; }
}

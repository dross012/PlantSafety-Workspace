// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-side DTO for a Crisis Management event record.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.SafetyEvents.Models.CrisisManagement;

/// <summary>
/// Read-side projection of a CrisisEvent aggregate.
/// A crisis event is a high-severity, multi-stakeholder safety emergency requiring coordinated response.
/// </summary>
public sealed record CrisisEventDto
{
    /// <summary>Unique identifier (UUIDv7).</summary>
    public required Guid CrisisEventId { get; init; }

    /// <summary>Tenant owning this record.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site affected by the crisis.</summary>
    public required Guid SiteId { get; init; }

    /// <summary>Short title. Max 200 characters.</summary>
    public required string Title { get; init; }

    /// <summary>Detailed description of the crisis.</summary>
    public required string Description { get; init; }

    /// <summary>Crisis category (e.g., Fire, ChemicalSpill, StructuralFailure, MassInjury).</summary>
    public required string CrisisCategory { get; init; }

    /// <summary>UTC timestamp when the crisis was declared.</summary>
    public required DateTimeOffset DeclaredAtUtc { get; init; }

    /// <summary>Current workflow status (e.g., Active, Contained, Resolved, ClosedOut).</summary>
    public required string Status { get; init; }

    /// <summary>Incident Commander for this crisis.</summary>
    public required Guid IncidentCommanderId { get; init; }

    /// <summary>UTC timestamp when the crisis was resolved, if applicable.</summary>
    public DateTimeOffset? ResolvedAtUtc { get; init; }

    /// <summary>UTC timestamp of last modification.</summary>
    public required DateTimeOffset LastModifiedAtUtc { get; init; }

    /// <summary>Row version for concurrency.</summary>
    public required string ETag { get; init; }
}

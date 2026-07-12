// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single PermitsToWork record.
// SchemaVersion: 1

namespace PlantSafety.Application.Operational.PermitsToWork.Models;

/// <summary>A single PermitsToWork read-model record.</summary>
public sealed record PermitsToWorkItemDto
{
    /// <summary>PermitsToWork record identifier.</summary>
    public required Guid PermitId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Type of work permit issued.</summary>
    public required string PermitType { get; init; }

    /// <summary>Description of the work being performed.</summary>
    public required string WorkDescription { get; init; }

    /// <summary>Current status of this record.</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when this record was last actioned.</summary>
    public required DateTime IssuedAtUtc { get; init; }

    /// <summary>Identity of the user who last actioned this record.</summary>
    public required Guid IssuedBy { get; init; }
}

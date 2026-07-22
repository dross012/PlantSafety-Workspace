// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Read-model item DTO for a single AutomatedIncidentDetection record. Automated incident detection: slip/fall, fire/smoke, unauthorized access, man-down, equipment failure, chemical spill.
// SchemaVersion: 1

namespace PlantSafety.Application.Observability.AutomatedIncidentDetection.Models;

/// <summary>A single AutomatedIncidentDetection read-model record.</summary>
public sealed record AutomatedIncidentDetectionItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid AutomatedIncidentDetectionId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current status (e.g., Created/Active/Completed/Closed/Archived).</summary>
    public required string Status { get; init; }

    /// <summary>DetectionType.</summary>
    public required  string DetectionType { get; init; }

    /// <summary>LocationDescription.</summary>
    public required  string LocationDescription { get; init; }

    /// <summary>NotifiedSupervisorUserId.</summary>
    public Guid? NotifiedSupervisorUserId { get; init; }

    /// <summary>UTC timestamp when the record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }
}
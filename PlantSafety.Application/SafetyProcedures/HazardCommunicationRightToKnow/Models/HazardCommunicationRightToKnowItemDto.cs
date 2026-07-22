// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Read-model item DTO for a single HazardCommunicationRightToKnow record.
// SchemaVersion: 1

namespace PlantSafety.Application.SafetyProcedures.HazardCommunicationRightToKnow.Models;

/// <summary>A single HazardCommunicationRightToKnow record (Hazard communication and right-to-know procedures ensure employees understand chemical/physical hazards via GHS labeling, accessible SDS, and hazard-handling training documentation.).</summary>
public sealed record HazardCommunicationRightToKnowItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid HazardCommunicationRightToKnowId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Procedure Title.</summary>
    public required string ProcedureTitle { get; init; }

    /// <summary>Classification: Chemical, Physical, Biological.</summary>
    public required string HazardClass { get; init; }

    /// <summary>Sds Reference.</summary>
    public required string SdsReference { get; init; }

    /// <summary>Overall status classification.</summary>
    public required string ProcedureStatus { get; init; }

    /// <summary>UTC timestamp when this record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }
}
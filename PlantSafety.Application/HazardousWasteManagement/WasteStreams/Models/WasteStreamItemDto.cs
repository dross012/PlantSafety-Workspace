// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-model item DTO for a single WasteStream record.
// SchemaVersion: 1

namespace PlantSafety.Application.HazardousWasteManagement.WasteStreams.Models;

/// <summary>A single characterized hazardous waste stream record.</summary>
public sealed record WasteStreamItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid WasteStreamId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Human-readable name of the waste stream.</summary>
    public required string StreamName { get; init; }

    /// <summary>EPA hazardous waste code for the current characterization version.</summary>
    public required string EpaWasteCode { get; init; }

    /// <summary>RCRA-style hazard classification.</summary>
    public required string HazardClass { get; init; }

    /// <summary>Generator's EPA identification number.</summary>
    public required string GeneratorEpaId { get; init; }

    /// <summary>Current characterization version.</summary>
    public required int CharacterizationVersion { get; init; }

    /// <summary>Overall status classification.</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when this record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }
}

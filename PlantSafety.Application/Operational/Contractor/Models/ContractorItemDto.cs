// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single Contractor record.
// SchemaVersion: 1

namespace PlantSafety.Application.Operational.Contractor.Models;

/// <summary>A single Contractor read-model record.</summary>
public sealed record ContractorItemDto
{
    /// <summary>Contractor record identifier.</summary>
    public required Guid ContractorRecordId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Name of the contractor organisation.</summary>
    public required string ContractorName { get; init; }

    /// <summary>Type of service the contractor provides.</summary>
    public required string ServiceType { get; init; }

    /// <summary>Current status of this record.</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when this record was last actioned.</summary>
    public required DateTime OnboardedAtUtc { get; init; }

    /// <summary>Identity of the user who last actioned this record.</summary>
    public required Guid ApprovedBy { get; init; }
}

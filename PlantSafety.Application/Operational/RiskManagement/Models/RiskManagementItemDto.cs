// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single RiskManagement record.
// SchemaVersion: 1

namespace PlantSafety.Application.Operational.RiskManagement.Models;

/// <summary>A single RiskManagement read-model record.</summary>
public sealed record RiskManagementItemDto
{
    /// <summary>RiskManagement record identifier.</summary>
    public required Guid RiskRecordId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Description of the identified risk.</summary>
    public required string RiskDescription { get; init; }

    /// <summary>Risk level classification (e.g., Low/Medium/High).</summary>
    public required string RiskLevel { get; init; }

    /// <summary>Current status of this record.</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when this record was last actioned.</summary>
    public required DateTime IdentifiedAtUtc { get; init; }

    /// <summary>Identity of the user who last actioned this record.</summary>
    public required Guid IdentifiedBy { get; init; }
}

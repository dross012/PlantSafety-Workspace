// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single WorkCenter record.
// SchemaVersion: 1

namespace PlantSafety.Application.CrossDomain.WorkCenters.Models;

/// <summary>A single production work center record.</summary>
public sealed record WorkCentersItemDto
{
    /// <summary>Work center identifier.</summary>
    public required Guid WorkCenterId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Work center display name.</summary>
    public required string WorkCenterName { get; init; }

    /// <summary>Work center code used in scheduling/routing.</summary>
    public required string WorkCenterCode { get; init; }

    /// <summary>Rated production capacity for the work center.</summary>
    public required int Capacity { get; init; }

    /// <summary>Current work center status (e.g., Active/Idle/Decommissioned).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the record was last updated.</summary>
    public required DateTime LastUpdatedAtUtc { get; init; }

    /// <summary>Identity of the user who last updated the record.</summary>
    public required Guid LastUpdatedBy { get; init; }
}

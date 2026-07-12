// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single PPEManagement record.
// SchemaVersion: 1

namespace PlantSafety.Application.Operational.PPEManagement.Models;

/// <summary>A single PPEManagement read-model record.</summary>
public sealed record PPEManagementItemDto
{
    /// <summary>PPEManagement record identifier.</summary>
    public required Guid PPERecordId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Employee this PPE record relates to.</summary>
    public required Guid EmployeeId { get; init; }

    /// <summary>Type of personal protective equipment.</summary>
    public required string PPEType { get; init; }

    /// <summary>Current status of this record.</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when this record was last actioned.</summary>
    public required DateTime IssuedAtUtc { get; init; }

    /// <summary>Identity of the user who last actioned this record.</summary>
    public required Guid IssuedBy { get; init; }
}

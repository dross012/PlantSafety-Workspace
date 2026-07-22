// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Read-model item DTO for a single PPECategory record. PPE categories and hazard alignment: head, eye/face, hearing, hand, foot, body/skin, respiratory, and fall protection categories.
// SchemaVersion: 1

namespace PlantSafety.Application.PPE.PPECategory.Models;

/// <summary>A single PPECategory read-model record.</summary>
public sealed record PPECategoryItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid PPECategoryId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current status (e.g., Created/Active/Completed/Closed/Archived).</summary>
    public required string Status { get; init; }

    /// <summary>CategoryCode.</summary>
    public required  string CategoryCode { get; init; }

    /// <summary>HazardsMitigated.</summary>
    public required  string HazardsMitigated { get; init; }

    /// <summary>InspectionIntervalDays.</summary>
    public required  int InspectionIntervalDays { get; init; }

    /// <summary>UTC timestamp when the record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }
}